import { QueryClient } from "@tanstack/react-query";
import { isApiError } from "@/lib/api";

// Retrying a 4xx (bad input, not allowed, not found) can never help. Network and 5xx get two retries.
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (isApiError(error) && error.status >= 400 && error.status < 500)
    return false;
  return failureCount < 2;
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: shouldRetry,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
});
