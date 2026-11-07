import { QueryClient } from "@tanstack/react-query";
import { isApiError } from "@/lib/api";
import { getTokens, onTokensChange } from "@/lib/tokens";

// Retrying a 4xx (bad input, not allowed, not found) can never help. Network and 5xx get two retries.
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (isApiError(error) && error.status >= 400 && error.status < 500)
    return false;
  return failureCount < 2;
}

// How long a page's data stays in memory once nothing on screen uses it. Going Back to a list within this time
// finds its rows (every page the person had loaded) waiting, instead of an empty list to load again.
export const KEEP_MS = 30 * 60_000;

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: shouldRetry,
      gcTime: KEEP_MS,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
});

// Signing out, or being signed out when a session expires, must not leave one person's data in
// memory for the next person on the same browser.
onTokensChange(() => {
  if (getTokens() === null) queryClient.clear();
});
