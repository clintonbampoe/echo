// Which face the verify page shows. Pure, so the rules are easy to read and to test.
export type SendState = "sending" | "sent" | "failed";
export type VerifyPhase =
  "verified" | "sending" | "failed" | "slow" | "waiting";

// After this long without the link being opened, say so and make "send another" easier to find.
export const SLOW_AFTER_MS = 45_000;

export function verifyPhase(f: {
  verified: boolean;
  send: SendState;
  slow: boolean;
}): VerifyPhase {
  if (f.verified) return "verified";
  if (f.send === "sending") return "sending";
  if (f.send === "failed") return "failed";
  return f.slow ? "slow" : "waiting";
}

// A link may go out by itself when the page opens, but not again on a refresh a moment later: the server
// limits how often it will send, and a person who reloads should see the countdown, not an error.
const RECENT_MS = 30_000;
const keyFor = (email: string) => `echo.verify.sent.${email}`;

export function sentRecently(email: string, now: number): boolean {
  try {
    const at = Number(sessionStorage.getItem(keyFor(email)));
    return at > 0 && now - at < RECENT_MS;
  } catch {
    return false;
  }
}

export function markSent(email: string, now: number) {
  try {
    sessionStorage.setItem(keyFor(email), String(now));
  } catch {
    // Private windows can refuse storage. Then a reload may send again, which is harmless.
  }
}
