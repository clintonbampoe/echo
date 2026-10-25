// What nearly every component test needs: a signed-in session, the app's providers, and canned answers.
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import type { ReactElement } from "react";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import { ToastProvider } from "@/ui";

export const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

// Starts a signed-in session (setup.ts clears it again after each test).
export function signIn() {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
}

// The path of a request, without the host or the /api prefix: "/v1/members/m1".
export const pathOf = (url: string) =>
  new URL(url, "http://x").pathname.replace("/api", "");

// Draws a component inside the providers the app has: data fetching, routing, toasts. Retries are off so a
// failing request fails the test at once.
export function renderApp(ui: ReactElement, route = "/") {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <MemoryRouter initialEntries={[route]}>{ui}</MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}
