import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderApp, signIn } from "@/test/kit";
import { pickDate } from "@/test/pickers";
import { NewMemberSheet } from "./NewMemberSheet";

type User = ReturnType<typeof userEvent.setup>;

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  signIn();
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

const posts = () =>
  fetchMock.mock.calls.filter(
    ([, init]) => (init as RequestInit | undefined)?.method === "POST",
  );

function renderSheet(onClose = vi.fn()) {
  renderApp(<NewMemberSheet onClose={onClose} />);
  return onClose;
}

// What a front-desk person fills in, then the details the server insists on, behind "More details".
async function fillEverything(user: User) {
  const type = (label: string, text: string) =>
    user.type(screen.getByLabelText(label), text);
  await type("Full name", "Kwame Asante");
  await type("Phone", "024 412 7788");
  await pickDate(user, "Date of birth", "1988-02-14");
  await user.click(screen.getByRole("button", { name: /More details/ }));
  await type("Residential address", "14 Palm Street");
  await type("City", "Accra");
  await type("Hometown", "Kumasi");
  await type("Next of kin", "Efua Asante");
  await type("Contact name", "Efua Asante");
  await type("Contact phone", "055 301 4408");
}

describe("add member sheet", () => {
  it("leads with the few things you have to hand, and folds the rest away", () => {
    renderSheet();
    expect(screen.getByLabelText("Full name")).toBeInTheDocument();
    expect(screen.getByLabelText("Phone")).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Female" })).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Male" })).toBeChecked();
    expect(
      screen.getByRole("button", { name: /More details/ }),
    ).toHaveAttribute("aria-expanded", "false");
  });

  it("names the church, and on an empty form says what to fix and opens the fold that hides some of it", async () => {
    const user = userEvent.setup();
    renderSheet();
    expect(
      await screen.findByText("Add someone to Grace Chapel."),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(await screen.findByText("Enter a first name")).toBeInTheDocument();
    expect(screen.getByText(/Fix \d+ fields to continue/)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /More details/ }),
    ).toHaveAttribute("aria-expanded", "true");
    expect(posts()).toHaveLength(0);
  });

  it("asks for a last name when only one name is typed", async () => {
    const user = userEvent.setup();
    renderSheet();
    await user.type(screen.getByLabelText("Full name"), "Abena");
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(await screen.findByText("Enter a last name")).toBeInTheDocument();
  });

  it("sends the name split into its parts, leaves empty optional fields out, says so, and closes", async () => {
    const user = userEvent.setup();
    const onClose = renderSheet();
    await fillEverything(user);
    await user.click(screen.getByRole("button", { name: "Add member" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());

    const post = posts()[0]!;
    const body = JSON.parse((post[1] as RequestInit).body as string) as Record<
      string,
      unknown
    >;
    expect(body).toMatchObject({
      firstName: "Kwame",
      lastName: "Asante",
      dateOfBirth: "1988-02-14",
      gender: "Male",
      region: "GreaterAccra",
      status: "Active",
    });
    expect(body).not.toHaveProperty("emailAddress");
    expect(body).not.toHaveProperty("fullName");
    expect(await screen.findByRole("status")).toHaveTextContent(
      "Kwame Asante added",
    );
  });

  it('keeps the sheet open on a blank form after "Save and add another"', async () => {
    const user = userEvent.setup();
    const onClose = renderSheet();
    await fillEverything(user);
    await user.click(
      screen.getByRole("button", { name: "Save and add another" }),
    );
    expect(await screen.findByRole("status")).toHaveTextContent(
      "Kwame Asante added",
    );
    expect(onClose).not.toHaveBeenCalled();
    await waitFor(() =>
      expect(screen.getByLabelText("Full name")).toHaveValue(""),
    );
    expect(screen.getByLabelText("Phone")).toHaveValue("");
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
    await fillEverything(user);
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(
      await screen.findByText(
        "The PhoneNumber field is not a valid phone number.",
      ),
    ).toBeInTheDocument();
  });
});
