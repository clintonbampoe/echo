import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import { MemberSheet } from "./MemberSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.includes("/congregation"))
      return json({ id: "2", name: "Grace Chapel" });
    if (init?.method === "POST")
      return json({ id: "m1", name: "Kwame Asante" }, 201);
    return json({});
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

function renderSheet(onClose = vi.fn()) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <MemberSheet open onClose={onClose} />
    </QueryClientProvider>,
  );
  return onClose;
}

async function fillRequired(user: ReturnType<typeof userEvent.setup>) {
  const type = (label: string, text: string) =>
    user.type(screen.getByLabelText(label), text);
  await type("First name", "Kwame");
  await type("Last name", "Asante");
  await type("Phone", "024 412 7788");
  await type("Date of birth", "1988-02-14");
  await type("Residential address", "14 Palm Street");
  await type("City", "Accra");
  await type("Hometown", "Kumasi");
  await type("Next of kin", "Efua Asante");
  await type("Contact name", "Efua Asante");
  await type("Contact phone", "055 301 4408");
}

describe("new member sheet", () => {
  it("names the church and says what to fix when the form is empty", async () => {
    const user = userEvent.setup();
    renderSheet();
    expect(
      await screen.findByText("Add someone to Grace Chapel."),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Create member" }));
    expect(await screen.findByText("Enter a first name")).toBeInTheDocument();
    expect(screen.getByText(/Fix \d+ fields to continue/)).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(
        ([, init]) => (init as RequestInit | undefined)?.method === "POST",
      ),
    ).toBe(false);
  });

  it("sends the member, leaving empty optional fields out, and closes", async () => {
    const user = userEvent.setup();
    const onClose = renderSheet();
    await fillRequired(user);
    await user.click(screen.getByRole("button", { name: "Create member" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const post = fetchMock.mock.calls.find(
      ([, init]) => (init as RequestInit | undefined)?.method === "POST",
    )!;
    const body = JSON.parse((post[1] as RequestInit).body as string) as Record<
      string,
      unknown
    >;
    expect(body).toMatchObject({
      firstName: "Kwame",
      dateOfBirth: "1988-02-14",
      gender: "Male",
      region: "GreaterAccra",
      status: "Active",
    });
    expect(body).not.toHaveProperty("emailAddress");
  });

  it('keeps the sheet open on a blank form after "Save and add another"', async () => {
    const user = userEvent.setup();
    const onClose = renderSheet();
    await fillRequired(user);
    await user.click(
      screen.getByRole("button", { name: "Save and add another" }),
    );
    expect(
      await screen.findByText("Kwame Asante was added."),
    ).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
    expect(screen.getByLabelText("First name")).toHaveValue("");
  });

  it("puts the server's message on the field it names", async () => {
    const user = userEvent.setup();
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      if (url.includes("/congregation"))
        return json({ id: "2", name: "Grace Chapel" });
      if (init?.method === "POST")
        return json(
          {
            errors: {
              PhoneNumber: [
                "The PhoneNumber field is not a valid phone number.",
              ],
            },
          },
          400,
        );
      return json({});
    });
    renderSheet();
    await fillRequired(user);
    await user.click(screen.getByRole("button", { name: "Create member" }));
    expect(
      await screen.findByText(
        "The PhoneNumber field is not a valid phone number.",
      ),
    ).toBeInTheDocument();
  });
});
