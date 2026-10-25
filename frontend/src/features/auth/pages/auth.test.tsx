import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  createMemoryRouter,
  RouterProvider,
  type RouteObject,
} from "react-router-dom";
import { getTokens } from "@/lib/tokens";
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

function renderAt(path: string, extra: RouteObject[] = []) {
  const router = createMemoryRouter([...authRoutes, ...extra], {
    initialEntries: [path],
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
  it("sends someone with no email to sign in", async () => {
    const router = renderAt("/verify");
    await waitFor(() => expect(router.state.location.pathname).toBe("/signin"));
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
