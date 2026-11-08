import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { StaffRolePage } from "./StaffRolePage";

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

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

function serve(
  rows: object[] = [person({}), person({ id: "u2", name: "Kofi Owusu" })],
) {
  const fetchMock = vi.fn(async (url: string) => {
    const path = pathOf(url);
    if (path === "/v1/users/me") return json({ id: "me", role: "Admin" });
    if (path === "/v1/users/summary")
      return json({
        total: 24,
        disabled: 1,
        roles: [
          { role: "Admin", count: 2, disabled: 0 },
          { role: "Clerk", count: 4, disabled: 1 },
          { role: "Accountant", count: 2, disabled: 0 },
          { role: "Member", count: 16, disabled: 0 },
        ],
      });
    if (path === "/v1/users")
      return json({ data: rows, hasMore: true, next: "c2" });
    return json({});
  });
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

const open = (route: string) =>
  renderApp(
    <Routes>
      <Route path="/staff" element={<p>Staff home</p>} />
      <Route path="/staff/:role" element={<StaffRolePage />} />
    </Routes>,
    route,
  );

describe("StaffRolePage", () => {
  it("shows the role's head counts from the summary and the people loaded so far", async () => {
    serve();
    open("/staff/clerk");
    expect(await screen.findByText("Efua Annan")).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Clerks" })).toBeInTheDocument();
    const strip = screen.getByLabelText("This role at a glance");
    expect(strip).toHaveTextContent("4");
    expect(strip).toHaveTextContent("17%");
    expect(strip).toHaveTextContent("1");
    // Only what is loaded is known, so that is all it claims.
    expect(screen.getByText("2 loaded")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Load more" }),
    ).toBeInTheDocument();
  });

  it("searches within the role", async () => {
    const fetchMock = serve();
    open("/staff/clerk");
    await screen.findByText("Efua Annan");
    await userEvent.type(screen.getByLabelText("Search clerks"), "efua{Enter}");
    await vi.waitFor(() =>
      expect(
        fetchMock.mock.calls.some(([url]) => {
          const u = new URL(url as string, "http://x");
          return (
            u.pathname.endsWith("/users") &&
            u.searchParams.get("role") === "Clerk" &&
            u.searchParams.get("q") === "efua"
          );
        }),
      ).toBe(true),
    );
  });

  it("goes back to Staff for an address that names no role", async () => {
    serve();
    open("/staff/nonsense");
    expect(await screen.findByText("Staff home")).toBeInTheDocument();
  });
});
