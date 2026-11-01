import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { roleLabel } from "@/features/account";
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
let profile: Record<string, unknown>;
beforeEach(() => {
  profile = me;
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string) => {
    if (url.includes("/users/me")) return json(profile);
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
    expect(within(sidebar).queryByText("Grace Chapel")).toBeNull();
    const account = within(sidebar).getByRole("link", { name: /Ama Mensah/ });
    expect(account).toHaveAttribute("href", "/me");
    expect(account).toHaveTextContent(roleLabel("Admin"));
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
      "Organizations",
      "Attendance",
      "Events",
      "Tithes",
      "Finance",
      "Projects",
      "Assets",
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

  it("shows Invitations at the end of the list to an administrator only", async () => {
    renderShell();
    const sidebar = screen.getByRole("complementary", { name: "Sidebar" });
    expect(
      await within(sidebar).findByRole("link", { name: "Invitations" }),
    ).toHaveAttribute("href", "/invitations");
    // One list, with no section headings.
    expect(within(sidebar).queryByText("People")).toBeNull();
    expect(
      within(sidebar)
        .getByRole("navigation", { name: "Main" })
        .querySelectorAll("a"),
    ).toHaveLength(11);
  });

  it("hides Invitations from anyone else", async () => {
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

describe("verify email reminder", () => {
  const unverified = () => {
    profile = { ...me, emailVerifiedAt: null };
  };
  const linkRequest = () =>
    fetchMock.mock.calls.find(([url]) =>
      String(url).includes("/auth/verifications/account"),
    );
  // Same answers as the default, except for the paths the test names.
  const answer = (overrides: Record<string, () => Response>) => {
    const normal = fetchMock.getMockImplementation() as (
      url: string,
      init?: RequestInit,
    ) => Promise<Response>;
    fetchMock.mockImplementation(async (url: string, init?: RequestInit) => {
      const hit = Object.entries(overrides).find(([path]) =>
        url.includes(path),
      );
      return hit ? hit[1]() : normal(url, init);
    });
  };

  it("asks an unverified user to confirm, and sends the link to their address", async () => {
    unverified();
    renderShell();
    expect(
      await screen.findByText("We have not confirmed ama@example.com yet."),
    ).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Send link" }));

    expect(await screen.findByText("Link on its way")).toBeInTheDocument();
    expect(
      JSON.parse((linkRequest()![1] as RequestInit).body as string),
    ).toEqual({ email: "ama@example.com" });
    expect(
      screen.getByRole("button", { name: "Resend in 0:30" }),
    ).toBeDisabled();
  });

  it("says nothing to a verified user", async () => {
    profile = { ...me, emailVerifiedAt: "2026-10-01T00:00:00Z" };
    renderShell();
    await screen.findByText("Ama Mensah");
    expect(
      screen.queryByText("Confirm your email address"),
    ).not.toBeInTheDocument();
  });

  it("says when the link could not be sent, and offers to try again", async () => {
    unverified();
    renderShell();
    await screen.findByText("Confirm your email address");
    answer({
      "/verifications/account": () => new Response(null, { status: 500 }),
    });
    await userEvent.click(screen.getByRole("button", { name: "Send link" }));

    expect(
      await screen.findByText("We could not send the link"),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Try again" })).toBeEnabled();
  });

  it("waits out a rate limit the way the resend button does", async () => {
    unverified();
    renderShell();
    await screen.findByText("Confirm your email address");
    answer({
      "/verifications/account": () => new Response(null, { status: 429 }),
    });
    await userEvent.click(screen.getByRole("button", { name: "Send link" }));

    expect(await screen.findByText("Link on its way")).toBeInTheDocument();
  });

  it("thanks the person once the address has been confirmed", async () => {
    unverified();
    renderShell();
    await screen.findByText("Confirm your email address");
    answer({
      "/verifications/status": () =>
        json({
          verified: true,
          verifiedAt: "2026-11-01T09:00:00Z",
          emailAddress: "ama@example.com",
        }),
    });
    await userEvent.click(screen.getByRole("button", { name: "Send link" }));

    expect(await screen.findByText("Email confirmed")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Resend/ })).toBeNull();
  });
});
