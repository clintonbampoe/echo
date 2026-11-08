import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { TeamPage } from "./TeamPage";

const person = (over: object) => ({
  id: "u1",
  name: "Efua Annan",
  emailAddress: "efua@echo.test",
  role: "Clerk",
  verifiedAt: "2026-03-03T10:00:00Z",
  disabledAt: null,
  createdAt: "2026-03-03T10:00:00Z",
  ...over,
});

const people: Record<string, object[]> = {
  Admin: [person({ id: "me", name: "Kwabena Ansah", role: "Admin" })],
  Clerk: [person({})],
  Accountant: [],
  Member: [
    person({
      id: "u2",
      name: "Naana Boateng",
      role: "Member",
      disabledAt: "2026-04-01T00:00:00Z",
    }),
  ],
};

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

function serve() {
  const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const path = pathOf(url);
    if (path === "/v1/users/me") return json({ id: "me", role: "Admin" });
    if (path === "/v1/users/summary")
      return json({
        total: 3,
        disabled: 1,
        roles: [
          { role: "Admin", count: 1, disabled: 0 },
          { role: "Accountant", count: 0, disabled: 0 },
          { role: "Clerk", count: 1, disabled: 0 },
          { role: "Member", count: 1, disabled: 1 },
        ],
      });
    if (path.endsWith("/disable") && init?.method === "POST")
      return json(person({ disabledAt: "2026-05-01T00:00:00Z" }));
    if (path === "/v1/users") {
      const role = new URL(url, "http://x").searchParams.get("role") ?? "";
      return json({ data: people[role] ?? [], hasMore: false, next: null });
    }
    return json({});
  });
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

describe("TeamPage", () => {
  it("groups people by role, flags a disabled account and counts them", async () => {
    serve();
    renderApp(<TeamPage />);
    expect(await screen.findByText("Efua Annan")).toBeInTheDocument();
    expect(screen.getByText("Naana Boateng")).toBeInTheDocument();
    expect(screen.getByText("Disabled")).toBeInTheDocument();
    expect(
      await screen.findByText("3 people · 1 disabled"),
    ).toBeInTheDocument();
    expect(screen.getByText("Admin only")).toBeInTheDocument();
  });

  it("marks your own row and gives it no way to change the account", async () => {
    serve();
    renderApp(<TeamPage />);
    expect(await screen.findByText("You")).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Actions for Kwabena Ansah" }),
    );
    expect(screen.getByText("View details")).toBeInTheDocument();
    expect(screen.queryByText("Disable account")).toBeNull();
  });

  it("disables an account after asking", async () => {
    const fetchMock = serve();
    renderApp(<TeamPage />);
    await userEvent.click(
      await screen.findByRole("button", { name: "Actions for Efua Annan" }),
    );
    await userEvent.click(screen.getByText("Disable account"));
    const dialog = await screen.findByRole("alertdialog");
    expect(dialog).toHaveTextContent("Disable Efua Annan's account?");
    await userEvent.click(
      within(dialog).getByRole("button", { name: "Disable account" }),
    );
    await vi.waitFor(() =>
      expect(
        fetchMock.mock.calls.some(
          ([url, init]) =>
            pathOf(url as string) === "/v1/users/u1/disable" &&
            (init as RequestInit).method === "POST",
        ),
      ).toBe(true),
    );
  });
});
