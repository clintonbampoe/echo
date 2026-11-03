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

// What a front-desk person fills in: the four things needed, then a few optional details behind "More details".
async function fillEverything(user: User) {
  const type = (label: string, text: string) =>
    user.type(screen.getByLabelText(label), text);
  await type("First name", "Kwame");
  await type("Last name", "Asante");
  await type("Phone", "024 412 7788");
  await pickDate(user, "Date of birth", "1988-02-14");
  await user.click(screen.getByRole("button", { name: /More details/ }));
  await type("Residential address", "14 Palm Street");
  await type("City", "Accra");
  await type("Hometown", "Kumasi");
}

async function fillRequired(user: User) {
  await user.type(screen.getByLabelText("First name"), "Kwame");
  await user.type(screen.getByLabelText("Last name"), "Asante");
  await user.type(screen.getByLabelText("Phone"), "024 412 7788");
  await pickDate(user, "Date of birth", "1988-02-14");
}

describe("add member sheet", () => {
  it("leads with the few things you have to hand, and folds the rest away", () => {
    renderSheet();
    expect(screen.getByLabelText("First name")).toBeInTheDocument();
    expect(screen.getByLabelText("Last name")).toBeInTheDocument();
    expect(screen.getByLabelText(/Other names/)).toBeInTheDocument();
    expect(screen.getByLabelText("Phone")).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Female" })).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Male" })).toBeChecked();
    expect(
      screen.getByRole("button", { name: /More details/ }),
    ).toHaveAttribute("aria-expanded", "false");
  });

  it("names the church, and on an empty form says what to fix", async () => {
    const user = userEvent.setup();
    renderSheet();
    expect(
      await screen.findByText("Add someone to Grace Chapel."),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(await screen.findByText("Enter a first name")).toBeInTheDocument();
    expect(screen.getByText(/Fix \d+ fields to continue/)).toBeInTheDocument();
    expect(posts()).toHaveLength(0);
  });

  it("needs only a name, phone, gender and birth date, and opens the fold when a detail in it is wrong", async () => {
    const user = userEvent.setup();
    const onClose = renderSheet();
    await fillRequired(user);
    await user.click(screen.getByRole("button", { name: /More details/ }));
    await user.type(screen.getByLabelText(/Email/), "kwame@");
    await user.click(screen.getByRole("button", { name: /More details/ }));
    await user.click(screen.getByRole("button", { name: "Add member" }));
    expect(
      await screen.findByText("Enter a valid email address"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /More details/ }),
    ).toHaveAttribute("aria-expanded", "true");
    await user.clear(screen.getByLabelText(/Email/));
    await user.click(screen.getByRole("button", { name: "Add member" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const body = JSON.parse(
      (posts()[0]![1] as RequestInit).body as string,
    ) as Record<string, unknown>;
    expect(body).toMatchObject({ firstName: "Kwame", gender: "Male" });
    expect(body).not.toHaveProperty("city");
    expect(body).not.toHaveProperty("region");
    expect(body).not.toHaveProperty("maritalStatus");
  });

  it("asks for a last name when only one name is typed", async () => {
    const user = userEvent.setup();
    renderSheet();
    await user.type(screen.getByLabelText("First name"), "Abena");
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
      city: "Accra",
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
      expect(screen.getByLabelText("First name")).toHaveValue(""),
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
