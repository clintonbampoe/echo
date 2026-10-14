import { render, screen, waitFor } from "@testing-library/react";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { RequireAuth } from "./RequireAuth";

const routes = [
  {
    Component: RequireAuth,
    children: [{ path: "private", element: <p>Private page</p> }],
  },
  { path: "signin", element: <p>Sign in page</p> },
];

function renderAt(path: string) {
  const router = createMemoryRouter(routes, { initialEntries: [path] });
  render(<RouterProvider router={router} />);
  return router;
}

describe("RequireAuth", () => {
  it("sends a signed-out visitor to sign in, remembering where they were going", async () => {
    const router = renderAt("/private?tab=2");
    await waitFor(() => expect(router.state.location.pathname).toBe("/signin"));
    expect(router.state.location.state).toEqual({ from: "/private?tab=2" });
  });

  it("lets a signed-in person through", () => {
    saveTokens({
      accessToken: "a",
      accessTokenExpiresAt: "2099-01-01T00:00:00Z",
      refreshToken: "r",
      refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
    });
    renderAt("/private");
    expect(screen.getByText("Private page")).toBeInTheDocument();
  });
});
