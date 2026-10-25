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
  it("shows the page and, in the sidebar, who is signed in and where", async () => {
    renderShell();
    expect(screen.getByText("Dashboard page")).toBeInTheDocument();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    expect(await within(sidebar).findByText("Ama Mensah")).toBeInTheDocument();
    expect(within(sidebar).getByText("Grace Chapel")).toBeInTheDocument();
    expect(
      within(sidebar).getByRole("link", { name: /Ama Mensah/ }),
    ).toHaveAttribute("href", "/me");
    expect(
      within(sidebar).getByRole("link", { name: "Settings" }),
    ).toHaveAttribute("href", "/settings");
  });

  it("links every page, in the order of the design, and marks the current one", () => {
    renderShell();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    const main = within(
      within(sidebar).getByRole("navigation", { name: "Main" }),
    );
    expect(main.getAllByRole("link").map((l) => l.textContent)).toEqual([
      "Dashboard",
      "Members",
      "Visitors",
      "Attendance",
      "Events",
      "Finance",
      "Tithes",
      "Projects",
      "Assets",
      "Organizations",
    ]);
    expect(main.getByRole("link", { name: "Dashboard" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(main.getByRole("link", { name: "Members" })).toHaveAttribute(
      "href",
      "/members",
    );
  });

  it("shows Invitations under Admin to an administrator only", async () => {
    renderShell();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    expect(
      await within(sidebar).findByRole("link", { name: "Invitations" }),
    ).toHaveAttribute("href", "/invitations");
    expect(within(sidebar).getByText("Admin")).toBeInTheDocument();
  });

  it("hides the Admin group from anyone else", async () => {
    me.role = "Clerk";
    renderShell();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    await within(sidebar).findByText("Ama Mensah");
    expect(
      within(sidebar).queryByRole("link", { name: "Invitations" }),
    ).toBeNull();
    me.role = "Admin";
  });

  it("opens the menu page on a phone, with the account, the pages, Settings and Sign out", async () => {
    const user = userEvent.setup();
    renderShell();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    const menu = screen.getByRole("dialog", { name: "Menu" });
    expect(
      await within(menu).findByText("Administrator · Grace Chapel"),
    ).toBeInTheDocument();
    expect(
      within(menu).getByRole("link", { name: "Dashboard" }),
    ).toBeInTheDocument();
    expect(
      within(menu).getByRole("link", { name: "Settings" }),
    ).toHaveAttribute("href", "/settings");
    expect(
      within(menu).getByRole("button", { name: "Sign out" }),
    ).toBeInTheDocument();
  });

  it("closes the menu with Escape, with the close button, and after following a link", async () => {
    const user = userEvent.setup();
    renderShell();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).toBeNull();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    await user.click(screen.getByRole("button", { name: "Close menu" }));
    expect(screen.queryByRole("dialog")).toBeNull();
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    await user.click(
      within(screen.getByRole("dialog")).getByRole("link", {
        name: "Dashboard",
      }),
    );
    expect(screen.queryByRole("dialog")).toBeNull();
  });

  it("has the logo go to the dashboard", () => {
    renderShell();
    for (const logo of screen.getAllByRole("link", { name: "Echo dashboard" }))
      expect(logo).toHaveAttribute("href", "/dashboard");
  });

  it("signs out from the menu and lands on sign in, without passing through anywhere else", async () => {
    const user = userEvent.setup();
    const router = renderShell();
    const visited: string[] = [];
    router.subscribe((state) => visited.push(state.location.pathname));
    await user.click(screen.getByRole("button", { name: "Open menu" }));
    await user.click(screen.getByRole("button", { name: "Sign out" }));
    expect(await screen.findByText("Sign in page")).toBeInTheDocument();
    await waitFor(() => expect(getTokens()).toBeNull());
    expect(visited.every((path) => path === "/signin")).toBe(true);
  });
});
