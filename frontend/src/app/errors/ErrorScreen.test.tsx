import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { createMemoryRouter, RouterProvider } from "react-router-dom";
import { ApiError } from "@/lib/api";
import { saveTokens } from "@/lib/tokens";
import { NotFound } from "../NotFound";
import { RouteError } from "./RouteError";

const json = (body: unknown) =>
  new Response(JSON.stringify(body), { status: 200 });

beforeEach(() => {
  vi.spyOn(console, "error").mockImplementation(() => {});
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) =>
      json(
        String(url).includes("/congregation")
          ? { id: "2", name: "Grace" }
          : { id: "1", name: "Ama Mensah", role: "Admin" },
      ),
    ),
  );
});
afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

function show(routes: Parameters<typeof createMemoryRouter>[0], at = "/") {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <RouterProvider
        router={createMemoryRouter(routes, { initialEntries: [at] })}
      />
    </QueryClientProvider>,
  );
}

const Boom = (error: unknown) => () => {
  throw error;
};

describe("error screens", () => {
  it("says a page is not found, with a way home, for someone not signed in", async () => {
    show([{ path: "*", Component: NotFound }], "/nowhere");
    expect(await screen.findByText("404 page not found")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Back to home" })).toHaveAttribute(
      "href",
      "/",
    );
    expect(screen.getByRole("button", { name: "Go back" })).toBeInTheDocument();
  });

  it("is a page of the website, without the app shell, even for someone signed in", async () => {
    saveTokens({
      accessToken: "a",
      accessTokenExpiresAt: "2099-01-01T00:00:00Z",
      refreshToken: "r",
      refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
    });
    show([{ path: "*", Component: NotFound }], "/nowhere");
    expect(await screen.findByText("404 page not found")).toBeInTheDocument();
    expect(screen.queryByRole("complementary", { name: "Sidebar" })).toBeNull();
    expect(
      screen.getByRole("link", { name: "Go to the dashboard" }),
    ).toHaveAttribute("href", "/dashboard");
  });

  it("explains a page that threw, and offers to try again", async () => {
    show([
      {
        path: "/",
        Component: Boom(new Error("boom")),
        ErrorBoundary: RouteError,
      },
    ]);
    expect(
      await screen.findByText("This page hit a problem"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Try again" }),
    ).toBeInTheDocument();
  });

  it("tells a person they may not open a page", async () => {
    show([
      {
        path: "/",
        Component: Boom(new ApiError(403, "FORBIDDEN", "no")),
        ErrorBoundary: RouteError,
      },
    ]);
    expect(
      await screen.findByText("You do not have access to this"),
    ).toBeInTheDocument();
  });

  it("tells a person who is offline", async () => {
    show([
      {
        path: "/",
        Component: Boom(new ApiError(0, "NETWORK", "x")),
        ErrorBoundary: RouteError,
      },
    ]);
    expect(
      await screen.findByText("You seem to be offline"),
    ).toBeInTheDocument();
  });

  it("asks an old tab to reload", async () => {
    show([
      {
        path: "/",
        Component: Boom(
          new TypeError("Failed to fetch dynamically imported module: /a.js"),
        ),
        ErrorBoundary: RouteError,
      },
    ]);
    expect(
      await screen.findByText("Echo has been updated"),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Reload" })).toBeInTheDocument();
  });
});
