import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  createMemoryRouter,
  RouterProvider,
  type RouteObject,
} from "react-router-dom";
import { getTokens, saveTokens } from "@/lib/tokens";
import { authRoutes } from "../routes";

const json = (status: number, body?: unknown) =>
  new Response(body === undefined ? null : JSON.stringify(body), { status });

const tokens = {
  accessToken: "a",
  accessTokenExpiresAt: "2099-01-01T00:00:00Z",
  refreshToken: "r",
  refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  fetchMock = vi.fn();
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

function renderAt(path: string, extra: RouteObject[] = [], state?: unknown) {
  const router = createMemoryRouter([...authRoutes, ...extra], {
    initialEntries: [state ? { pathname: path, state } : path],
  });
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

describe("sign in", () => {
  it("stores the session and goes to the page that asked for it", async () => {
    fetchMock.mockResolvedValueOnce(json(200, tokens));
    const router = renderAt("/signin", [
      { path: "members", element: <p>Members page</p> },
    ]);
    await router.navigate("/signin", { state: { from: "/members" } });

    await userEvent.type(
      await screen.findByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "secret-pass");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByText("Members page")).toBeInTheDocument();
    expect(getTokens()?.refreshToken).toBe("r");
  });

  it("ignores a return path that points off-site", async () => {
    fetchMock.mockResolvedValueOnce(json(200, tokens));
    const router = renderAt("/signin", [
      { path: "dashboard", element: <p>Dashboard page</p> },
    ]);
    await router.navigate("/signin", { state: { from: "//evil.example" } });

    await userEvent.type(
      await screen.findByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "secret-pass");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByText("Dashboard page")).toBeInTheDocument();
  });

  it("shows one message for a wrong email or password", async () => {
    fetchMock.mockResolvedValueOnce(
      json(401, { errorCode: "INVALID_CREDENTIALS", message: "x" }),
    );
    renderAt("/signin");

    await userEvent.type(
      await screen.findByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "wrong-pass");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(
      await screen.findByText("Email or password is incorrect"),
    ).toBeInTheDocument();
    expect(getTokens()).toBeNull();
  });

  it("checks the form before calling the server", async () => {
    renderAt("/signin");
    await userEvent.click(
      await screen.findByRole("button", { name: "Sign in" }),
    );
    expect(
      await screen.findByText("Enter your email address."),
    ).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});

describe("sign up", () => {
  it("sends someone who skipped step 1 back to it", async () => {
    const router = renderAt("/signup/church");
    await waitFor(() => expect(router.state.location.pathname).toBe("/signup"));
  });

  it("carries step 1 through to step 2", async () => {
    const router = renderAt("/signup");
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "secret-pass");
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    expect(await screen.findByLabelText("Church name")).toBeInTheDocument();
    expect(router.state.location.pathname).toBe("/signup/church");
  });
});

describe("verify", () => {
  const sent = (status: unknown = { verified: false }) =>
    fetchMock.mockImplementation(async (url: string) =>
      String(url).includes("/verifications/status")
        ? json(200, status)
        : json(200),
    );
  const linkRequests = () =>
    fetchMock.mock.calls.filter(([url]) =>
      String(url).includes("/verifications/account"),
    );
  beforeEach(() => sessionStorage.clear());

  it("asks a new user to check their email, and lets them skip to sign-in", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@example.com" });
    expect(await screen.findByText("Check your email")).toBeInTheDocument();
    expect(screen.getByText("ama@example.com")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Skip for now/ })).toHaveAttribute(
      "href",
      "/signin",
    );
  });

  it("shows the address as the one they signed up with, and never as a field", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@example.com" });
    await screen.findByText("Check your email");
    expect(
      screen.getByText("Sent to the email you signed up with"),
    ).toBeInTheDocument();
    expect(screen.queryByRole("textbox")).toBeNull();
  });

  it("does not send again when the link went out at sign-up", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@example.com" });
    expect(
      await screen.findByText("Send another link in 0:30"),
    ).toBeInTheDocument();
    expect(linkRequests()).toHaveLength(0);
  });

  it("offers a shortcut to the mail of a known provider, in a new tab", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@gmail.com" });
    const open = await screen.findByRole("link", { name: /Open Gmail/ });
    expect(open).toHaveAttribute("href", "https://mail.google.com");
    expect(open).toHaveAttribute("target", "_blank");
  });

  it("offers no shortcut for an address on a domain of its own", async () => {
    sent();
    renderAt("/verify", [], { email: "kwame@gracechapel.org" });
    await screen.findByText("Check your email");
    expect(screen.queryByRole("link", { name: /Open / })).toBeNull();
  });

  it("sends the link by itself when the reminder sends them here, then waits", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@example.com", send: true });
    expect(
      await screen.findByText("Waiting for you to open the link"),
    ).toBeInTheDocument();
    expect(linkRequests()).toHaveLength(1);
    expect(
      JSON.parse((linkRequests()[0]![1] as RequestInit).body as string),
    ).toEqual({ email: "ama@example.com" });
    expect(screen.getByText("Send another link in 0:30")).toBeInTheDocument();
  });

  it("does not send a second time when the page is reloaded straight away", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@example.com", send: true });
    await screen.findByText("Waiting for you to open the link");
    cleanup();
    renderAt("/verify", [], { email: "ama@example.com", send: true });
    await screen.findByText("Waiting for you to open the link");
    expect(linkRequests()).toHaveLength(1);
  });

  it("says when the link could not be sent, and sends it on a second try", async () => {
    fetchMock.mockResolvedValueOnce(json(500));
    renderAt("/verify", [], { email: "ama@example.com", send: true });
    expect(
      await screen.findByText("We could not send the link"),
    ).toBeInTheDocument();
    sent();
    await userEvent.click(screen.getByRole("button", { name: "Try again" }));
    expect(
      await screen.findByText("Waiting for you to open the link"),
    ).toBeInTheDocument();
  });

  it("waits out a rate limit as if the link had been sent", async () => {
    fetchMock.mockResolvedValueOnce(json(429));
    renderAt("/verify", [], { email: "ama@example.com", send: true });
    expect(
      await screen.findByText("Waiting for you to open the link"),
    ).toBeInTheDocument();
  });

  it("sends another link on request and shows the wait again", async () => {
    sent();
    renderAt("/verify", [], { email: "ama@gmail.com", send: true });
    await screen.findByText("Send another link in 0:30");
    expect(linkRequests()).toHaveLength(1);
  });

  it("lets someone who is signed in go back to the app", async () => {
    saveTokens(tokens);
    sent();
    renderAt("/verify", [{ path: "dashboard", element: <p>Dashboard</p> }], {
      email: "ama@example.com",
    });
    expect(
      await screen.findByRole("link", { name: /Back to the app/ }),
    ).toHaveAttribute("href", "/dashboard");
  });

  it("notices by itself that the account was verified, and offers the way on", async () => {
    saveTokens(tokens);
    sent({ verified: true, verifiedAt: "2026-11-01T09:00:00Z" });
    renderAt("/verify", [{ path: "dashboard", element: <p>Dashboard</p> }], {
      email: "ama@example.com",
    });
    expect(
      await screen.findByRole("heading", { name: "Account verified" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /Go to the dashboard/ }),
    ).toHaveAttribute("href", "/dashboard");
    expect(screen.getByText("Verified")).toBeInTheDocument();
  });

  it("goes on to the app by itself after a moment", async () => {
    saveTokens(tokens);
    sent({ verified: true });
    renderAt("/verify", [{ path: "dashboard", element: <p>Dashboard</p> }], {
      email: "ama@example.com",
    });
    expect(await screen.findByText("Continuing in 3")).toBeInTheDocument();
    expect(
      await screen.findByText("Dashboard", {}, { timeout: 5000 }),
    ).toBeInTheDocument();
  }, 8000);

  it("stays on the page when the person says so", async () => {
    saveTokens(tokens);
    sent({ verified: true });
    renderAt("/verify", [], { email: "ama@example.com" });
    await userEvent.click(
      await screen.findByRole("button", { name: "Stay here" }),
    );
    expect(screen.queryByText(/Continuing in/)).toBeNull();
    expect(
      screen.getByRole("heading", { name: "Account verified" }),
    ).toBeInTheDocument();
  });

  it("hears from another tab that opened the link, even without a session", async () => {
    class Channel {
      static all: Channel[] = [];
      onmessage: (() => void) | null = null;
      constructor() {
        Channel.all.push(this);
      }
      postMessage() {
        Channel.all.forEach((c) => c.onmessage?.());
      }
      close() {
        Channel.all = Channel.all.filter((c) => c !== this);
      }
    }
    vi.stubGlobal("BroadcastChannel", Channel);
    sent();
    renderAt("/verify", [], { email: "ama@example.com" });
    await screen.findByText("Check your email");
    act(() => new Channel().postMessage());
    expect(
      await screen.findByRole("heading", { name: "Account verified" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Sign in/ })).toHaveAttribute(
      "href",
      "/signin",
    );
  });

  it("sends someone with no email to sign in", async () => {
    const router = renderAt("/verify");
    await waitFor(() => expect(router.state.location.pathname).toBe("/signin"));
  });
});

describe("joining signs the new person in", () => {
  const answer = (login: Response) =>
    fetchMock.mockImplementation(async (url: string) => {
      if (String(url).includes("/sessions/login")) return login;
      if (String(url).includes("/registrations/member")) return json(200);
      return json(200, {
        congregationName: "Grace Chapel",
        city: "Osu",
        region: "GreaterAccra",
        allowedRole: "Member",
      });
    });

  async function fillAndSubmit() {
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "secret-pass");
    await userEvent.click(
      screen.getByRole("button", { name: /Create account|Join/ }),
    );
  }

  it("keeps the session it was given and offers to go back to the app", async () => {
    answer(json(200, tokens));
    renderAt("/join/abc/account");
    await fillAndSubmit();
    expect(await screen.findByText("Check your email")).toBeInTheDocument();
    expect(getTokens()?.refreshToken).toBe("r");
    expect(
      screen.getByRole("link", { name: /Back to the app/ }),
    ).toHaveAttribute("href", "/dashboard");
  });

  it("still gets there when the automatic sign-in fails, and skips to sign-in", async () => {
    answer(json(500, { title: "Server error", status: 500 }));
    renderAt("/join/abc/account");
    await fillAndSubmit();
    expect(await screen.findByText("Check your email")).toBeInTheDocument();
    expect(getTokens()).toBeNull();
    expect(screen.getByRole("link", { name: /Skip for now/ })).toHaveAttribute(
      "href",
      "/signin",
    );
  });
});

describe("invitations", () => {
  it("tells the person when a code is no longer valid", async () => {
    fetchMock.mockResolvedValueOnce(
      json(404, { errorCode: "NOT_FOUND", message: "x" }),
    );
    renderAt("/join/abc");
    expect(
      await screen.findByText("This invitation is no longer valid"),
    ).toBeInTheDocument();
  });

  const grace = {
    congregationName: "Grace Chapel",
    city: "Osu",
    region: "GreaterAccra",
    allowedRole: "Member",
    invitedBy: "Pastor Kwame",
  };

  it("asks the person to confirm which church the invitation is for", async () => {
    fetchMock.mockResolvedValueOnce(json(200, grace));
    renderAt("/join/abc");
    expect(await screen.findByText("Grace Chapel")).toBeInTheDocument();
    expect(screen.getByText(/Osu, Greater Accra/)).toBeInTheDocument();
    expect(screen.getByText("Invited by Pastor Kwame")).toBeInTheDocument();
  });

  it("goes on to the account form once the church is confirmed", async () => {
    fetchMock.mockResolvedValue(json(200, grace));
    const router = renderAt("/join/abc");
    await userEvent.click(
      await screen.findByRole("button", { name: "Yes, continue" }),
    );
    expect(await screen.findByLabelText("First name")).toBeInTheDocument();
    expect(router.state.location.pathname).toBe("/join/abc/account");
    expect(
      screen.getByText(
        "Pastor Kwame invited you to join Grace Chapel on Echo.",
      ),
    ).toBeInTheDocument();
  });

  it("goes back to the code when it is not their church", async () => {
    fetchMock.mockResolvedValueOnce(json(200, grace));
    const router = renderAt("/join/abc");
    await userEvent.click(
      await screen.findByRole("button", { name: "This is not my church" }),
    );
    await waitFor(() => expect(router.state.location.pathname).toBe("/join"));
  });

  it("checks a typed code, then shows the church to confirm", async () => {
    fetchMock.mockResolvedValue(json(200, grace));
    const router = renderAt("/join");
    await userEvent.type(
      await screen.findByLabelText("Invitation code"),
      "GRACE-4821",
    );
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));
    await waitFor(() =>
      expect(router.state.location.pathname).toBe("/join/GRACE-4821"),
    );
    expect(await screen.findByText("Is this your church?")).toBeInTheDocument();
  });
});

describe("light and dark", () => {
  it("lets the person switch an auth screen between light and dark", async () => {
    localStorage.clear();
    renderAt("/signin");
    await userEvent.click(
      await screen.findByRole("button", {
        name: /switch to (dark|light) mode/i,
      }),
    );
    const chosen = localStorage.getItem("echo-theme");
    expect(chosen === "light" || chosen === "dark").toBe(true);
    expect(document.documentElement.dataset.surface).toBe(chosen);
  });
});

describe("links on auth screens", () => {
  it("offers a way back to the landing page", async () => {
    renderAt("/signin");
    expect(
      await screen.findByRole("link", { name: "Back to home" }),
    ).toHaveAttribute("href", "/");
  });

  it("offers the other ways in as plain links under the sign-in form", async () => {
    renderAt("/signin");
    expect(
      await screen.findByRole("link", { name: "Create a church account" }),
    ).toHaveAttribute("href", "/signup");
    expect(
      screen.getByRole("link", { name: "Join with an invitation code" }),
    ).toHaveAttribute("href", "/join");
  });
});
