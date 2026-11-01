// Where someone was when they asked to verify their account, so they can be taken back there afterwards.

// Only same-site paths are accepted, so a crafted link or stale storage cannot send someone elsewhere.
export function safePath(value: unknown): string | undefined {
  return typeof value === "string" &&
    value.startsWith("/") &&
    !value.startsWith("//")
    ? value
    : undefined;
}

const KEY = "echo.verify.from";

// The email link opens in a tab of its own, which cannot see this tab's navigation state, so the page is
// also kept in local storage for that tab to read.
export function rememberOrigin(path: string | undefined) {
  if (!path) return forgetOrigin();
  try {
    localStorage.setItem(KEY, path);
  } catch {
    // Storage can be refused. Then the dashboard is where they land, which is fine.
  }
}

export function recallOrigin(): string | undefined {
  try {
    return safePath(localStorage.getItem(KEY));
  } catch {
    return undefined;
  }
}

export function forgetOrigin() {
  try {
    localStorage.removeItem(KEY);
  } catch {
    // Nothing to forget.
  }
}
