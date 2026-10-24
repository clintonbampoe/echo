import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import type { ReactElement } from "react";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";

export const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

// A request's path with the dev "/api" prefix taken off, so mocks match what a card asks for.
export const asked = (url: string | URL | Request) => {
  const u = new URL(url.toString(), "http://x");
  return {
    path: u.pathname.replace(/^\/api(?=\/)/, ""),
    query: u.searchParams,
  };
};

export function signIn() {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
}

// A card on its own, with a router (its title is a link) and a query client that does not retry.
export const renderCard = (ui: ReactElement) =>
  render(
    <MemoryRouter>
      <QueryClientProvider
        client={
          new QueryClient({ defaultOptions: { queries: { retry: false } } })
        }
      >
        {ui}
      </QueryClientProvider>
    </MemoryRouter>,
  );
