import { useSyncExternalStore } from "react";
import { getTokens, onTokensChange } from "@/lib/tokens";

// Signed in means a refresh token that has not expired. The access token may be stale;
// the API client refreshes it when needed.
function hasSession(): boolean {
  const tokens = getTokens();
  return (
    tokens !== null &&
    new Date(tokens.refreshTokenExpiresAt).getTime() > Date.now()
  );
}

export function useIsSignedIn(): boolean {
  return useSyncExternalStore(onTokensChange, hasSession);
}
