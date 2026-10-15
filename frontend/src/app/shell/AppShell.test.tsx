import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { getTokens, saveTokens } from "@/lib/tokens";
import { AppShell } from "./AppShell";

const json = (body: unknown) =>
  new Response(JSON.stringify(body), { status: 200 });

const me = {
  id: "1",
  name: "Ama Mensah",
  emailAddress: "ama@example.com",
  role: "Admin",
};
const church = { id: "2", name: "Grace Chapel" };

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string) => {
    if (url.includes("/users/me")) return json(me);
    if (url.includes("/congregation")) return json(church);
    return new Response(null, { status: 204 });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

function renderShell() {
  const router = createMemoryRouter(
    [
      {
        Component: AppShell,
        children: [{ path: "dashboard", element: <p>Dashboard page</p> }],
      },
      { path: "signin", element: <p>Sign in page</p> },
      { path: "/", element: <p>Home page</p> },
    ],
    { initialEntries: ["/dashboard"] },
  );
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );
  return router;
}

describe("app shell", () => {
  it("shows the page, the church and the signed-in person", async () => {
    renderShell();
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
    expect(await screen.findByText("Grace Chapel")).toBeInTheDocument();
    expect(screen.getByText("Administrator")).toBeInTheDocument();
    expect(screen.getByText("Ama Mensah")).toBeInTheDocument();
    expect(screen.getByText("ama@example.com")).toBeInTheDocument();
  });

  it("links pages that exist and marks the rest as coming soon", () => {
    renderShell();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    const current = within(sidebar).getByRole("link", { name: "Dashboard" });
    expect(current).toHaveAttribute("aria-current", "page");
    expect(
      within(sidebar).getByRole("link", { name: "Members" }),
    ).toHaveAttribute("href", "/members");
    expect(
      within(sidebar).getByRole("link", { name: "Visitors" }),
    ).toHaveAttribute("href", "/visitors");
    expect(
      within(sidebar).getByRole("link", { name: "Organizations" }),
    ).toHaveAttribute("href", "/organizations");
    expect(
      within(sidebar).queryByRole("link", { name: /Attendance/ }),
    ).toBeNull();
    expect(
      within(sidebar).getByText("Reports").closest("[aria-disabled]"),
    ).toHaveAttribute("aria-disabled", "true");
  });

  it("opens the same menu in a drawer on a phone, and Escape closes it", async () => {
    const user = userEvent.setup();
    renderShell();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    const drawer = screen.getByRole("dialog", { name: "Menu" });
    expect(
      within(drawer).getByRole("link", { name: "Dashboard" }),
    ).toBeInTheDocument();
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).toBeNull();
  });

  it("closes the drawer after following a link", async () => {
    const user = userEvent.setup();
    renderShell();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    await user.click(
      within(screen.getByRole("dialog")).getByRole("link", {
        name: "Dashboard",
      }),
    );
    expect(screen.queryByRole("dialog")).toBeNull();
  });

  it("has the logo go to the dashboard and the account menu go to the homepage", async () => {
    const user = userEvent.setup();
    renderShell();
    expect(
      screen.getByRole("link", { name: "Echo dashboard" }),
    ).toHaveAttribute("href", "/dashboard");
    await user.click(await screen.findByRole("button", { name: /Ama Mensah/ }));
    expect(screen.getByRole("menuitem", { name: "Homepage" })).toHaveAttribute(
      "href",
      "/",
    );
  });

  it("signs out from the account menu and lands on sign in, without passing through anywhere else", async () => {
    const user = userEvent.setup();
    const router = renderShell();
    const visited: string[] = [];
    router.subscribe((state) => visited.push(state.location.pathname));
    await user.click(await screen.findByRole("button", { name: /Ama Mensah/ }));
    await user.click(screen.getByRole("menuitem", { name: "Sign out" }));
    expect(await screen.findByText("Sign in page")).toBeInTheDocument();
    await waitFor(() => expect(getTokens()).toBeNull());
    expect(visited.every((path) => path === "/signin")).toBe(true);
  });
});
