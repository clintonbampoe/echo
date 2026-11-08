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

// A fresh value for each use, so no password is written into the test source.
const makePassword = () => crypto.randomUUID();

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

  it("says the server is busy, and for how long, when many people are signing in", async () => {
    fetchMock.mockResolvedValueOnce(
      json(503, {
        errorCode: "SERVICE_BUSY",
        detail: "We are busy right now. Please try again in 7 seconds.",
      }),
    );
    renderAt("/signin");

    await userEvent.type(
      await screen.findByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), "right-pass");
    await userEvent.click(screen.getByRole("button", { name: "Sign in" }));

    expect(
      await screen.findByText("We are busy right now"),
    ).toBeInTheDocument();
    expect(screen.getByText(/try again in 7 seconds/)).toBeInTheDocument();
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
    const password = makePassword();
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), password);
    await userEvent.type(screen.getByLabelText("Confirm password"), password);
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    expect(await screen.findByLabelText("Church name")).toBeInTheDocument();
    expect(router.state.location.pathname).toBe("/signup/church");
  });

  it("on a phone, asks for the church in three short parts", async () => {
    vi.stubGlobal("matchMedia", (q: string) => ({
      matches: q.includes("599"),
      addEventListener: () => {},
      removeEventListener: () => {},
    }));
    renderAt("/signup");
    const password = makePassword();
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), password);
    await userEvent.type(screen.getByLabelText("Confirm password"), password);
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    // Part 1: the name. It cannot be skipped, and nothing from the other parts is on screen.
    await screen.findByLabelText("Church name");
    expect(screen.queryByLabelText("Phone")).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));
    expect(
      await screen.findByText("Enter your church name."),
    ).toBeInTheDocument();

    await userEvent.type(screen.getByLabelText("Church name"), "Grace Chapel");
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));
    expect(await screen.findByLabelText("Phone")).toBeInTheDocument();
    expect(screen.queryByLabelText("Church name")).not.toBeInTheDocument();

    // Back keeps what was typed.
    await userEvent.click(screen.getByRole("button", { name: "Back" }));
    expect(await screen.findByLabelText("Church name")).toHaveValue(
      "Grace Chapel",
    );
  });

  it("does not leave step 1 when the two passwords differ", async () => {
    const router = renderAt("/signup");
    const password = makePassword();
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), password);
    await userEvent.type(
      screen.getByLabelText("Confirm password"),
      makePassword(),
    );
    await userEvent.click(screen.getByRole("button", { name: "Continue" }));

    expect(
      await screen.findByText("The passwords do not match."),
    ).toBeInTheDocument();
    expect(router.state.location.pathname).toBe("/signup");
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
    // The request goes out from an effect, so it can land just after the wait is drawn.
    await waitFor(() => expect(linkRequests()).toHaveLength(1));
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

  // A stand-in for tabs of one browser: a message reaches every open channel.
  class Channel {
    static all: Channel[] = [];
    onmessage: ((e: { data: unknown }) => void) | null = null;
    constructor() {
      Channel.all.push(this);
    }
    postMessage(data: unknown) {
      Channel.all.forEach((c) => c.onmessage?.({ data }));
    }
    close() {
      Channel.all = Channel.all.filter((c) => c !== this);
    }
  }

  it("hears from another tab that opened the link, even without a session", async () => {
    vi.stubGlobal("BroadcastChannel", Channel);
    sent();
    renderAt("/verify", [], { email: "ama@example.com" });
    await screen.findByText("Check your email");
    // The page starts listening from an effect; wait until it has, or the message goes unheard.
    await waitFor(() =>
      expect(Channel.all.some((c) => c.onmessage)).toBe(true),
    );
    act(() => new Channel().postMessage("verified"));
    expect(
      await screen.findByRole("heading", { name: "Account verified" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Sign in/ })).toHaveAttribute(
      "href",
      "/signin",
    );
  });

  it("takes someone back to the page they asked from", async () => {
    saveTokens(tokens);
    sent({ verified: true });
    renderAt("/verify", [], { email: "ama@example.com", from: "/members" });
    expect(
      await screen.findByRole("link", { name: /Back to where you were/ }),
    ).toHaveAttribute("href", "/members");
  });

  it("ignores a page that points off-site", async () => {
    saveTokens(tokens);
    sent({ verified: true });
    renderAt("/verify", [], {
      email: "ama@example.com",
      from: "//evil.example",
    });
    expect(
      await screen.findByRole("link", { name: /Go to the dashboard/ }),
    ).toHaveAttribute("href", "/dashboard");
  });

  describe("the page the email link opens", () => {
    let close: ReturnType<typeof vi.fn>;
    beforeEach(() => {
      localStorage.setItem("echo.verify.from", "/members");
      // jsdom really closes its window, which ends the test run.
      close = vi.fn();
      vi.stubGlobal("close", close);
    });
    afterEach(() => localStorage.clear());

    it("says so and hands over when the waiting tab is still open", async () => {
      vi.stubGlobal("BroadcastChannel", Channel);
      const waiting = new Channel();
      waiting.onmessage = (e) => {
        if (e.data === "verified") waiting.postMessage("ack");
      };
      fetchMock.mockResolvedValue(json(200));
      renderAt("/verify-email?token=t");
      await userEvent.click(
        await screen.findByRole("button", { name: "Confirm my email" }),
      );
      expect(
        await screen.findByText(
          "Your other tab already knows. You can close this one.",
        ),
      ).toBeInTheDocument();
      expect(
        screen.getByRole("link", { name: "Continue here instead" }),
      ).toHaveAttribute("href", "/members");
      expect(close).toHaveBeenCalled();
    });

    it("takes the person back to where they started when it is the only tab", async () => {
      vi.stubGlobal("BroadcastChannel", undefined);
      fetchMock.mockResolvedValue(json(200));
      renderAt("/verify-email?token=t");
      await userEvent.click(
        await screen.findByRole("button", { name: "Confirm my email" }),
      );
      expect(
        await screen.findByRole("link", { name: /Sign in|Back to where/ }),
      ).toHaveAttribute("href", "/members");
    });

    it("does not spend the link until the person confirms", async () => {
      fetchMock.mockResolvedValue(json(200));
      renderAt("/verify-email?token=t");
      expect(
        await screen.findByRole("button", { name: "Confirm my email" }),
      ).toBeInTheDocument();
      expect(fetchMock).not.toHaveBeenCalled();
    });
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
    const password = makePassword();
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), password);
    await userEvent.type(screen.getByLabelText("Confirm password"), password);
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

  it("does not register anyone when the two passwords differ", async () => {
    answer(json(200, tokens));
    renderAt("/join/abc/account");
    const password = makePassword();
    await userEvent.type(await screen.findByLabelText("First name"), "Ama");
    await userEvent.type(screen.getByLabelText("Last name"), "Mensah");
    await userEvent.type(
      screen.getByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.type(screen.getByLabelText("Password"), password);
    await userEvent.type(
      screen.getByLabelText("Confirm password"),
      makePassword(),
    );
    await userEvent.click(screen.getByRole("button", { name: /Join/ }));

    expect(
      await screen.findByText("The passwords do not match."),
    ).toBeInTheDocument();
    expect(
      fetchMock.mock.calls.some(([url]) =>
        String(url).includes("/registrations/member"),
      ),
    ).toBe(false);
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
      await screen.findByRole("link", { name: "Back to site" }),
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

describe("the layout follows the screen", () => {
  afterEach(() => vi.unstubAllGlobals());
  const narrow = () =>
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener: () => {},
      removeEventListener: () => {},
    }));

  it("shows every other way in, as rows, on a wide screen", async () => {
    renderAt("/signin");
    expect(
      await screen.findByRole("heading", { name: "New to Echo?" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Join with an invitation code" }),
    ).toBeInTheDocument();
  });

  it("shows only the main link on a tablet or phone", async () => {
    narrow();
    renderAt("/signin");
    expect(
      await screen.findByRole("link", { name: "Create a church account" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("link", { name: "Join with an invitation code" }),
    ).not.toBeInTheDocument();
  });
});

describe("small kindnesses", () => {
  it("finishes a mail domain on Tab", async () => {
    renderAt("/signup");
    const email = await screen.findByLabelText("Email address");
    await userEvent.type(email, "abena@gm");
    await userEvent.tab();
    expect(email).toHaveValue("abena@gmail.com");
  });

  it("carries the typed email from sign in to forgot password", async () => {
    renderAt("/signin");
    await userEvent.type(
      await screen.findByLabelText("Email address"),
      "ama@example.com",
    );
    await userEvent.click(
      screen.getByRole("link", { name: "Forgot password?" }),
    );
    // The sign-in page has the same label, so wait for the forgot page's own button first.
    const change = await screen.findByRole("button", { name: "Change" });
    const email = screen.getByLabelText("Email address");
    expect(email).toHaveValue("ama@example.com");
    // It is shown, not asked for again: locked until the person chooses to change it.
    expect(email).toHaveAttribute("readonly");
    await userEvent.click(change);
    expect(screen.getByLabelText("Email address")).not.toHaveAttribute(
      "readonly",
    );
  });

  it("asks for the email on forgot password when none was carried", async () => {
    renderAt("/forgot");
    const email = await screen.findByLabelText("Email address");
    expect(email).not.toHaveAttribute("readonly");
    expect(screen.queryByRole("button", { name: "Change" })).toBeNull();
  });
});
