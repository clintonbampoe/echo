// Where the session tokens live. Plain functions, no React.
//
// Trade-off, on purpose: tokens are kept in localStorage so a session survives a reload.
// That means any script injection could read them. The CSP in nginx and the ban on
// dangerouslySetInnerHTML are what keep that risk small. Revisit if the backend moves
// refresh tokens to httpOnly cookies.

export type TokenPair = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
};

const KEY = "echo.session";

type Listener = () => void;
const listeners = new Set<Listener>();

// Parsed once and kept in memory, so requests do not re-read storage.
let cache: TokenPair | null | undefined;

function read(): TokenPair | null {
  try {
    const raw = localStorage.getItem(KEY);
    return raw ? (JSON.parse(raw) as TokenPair) : null;
  } catch {
    return null;
  }
}

function notify() {
  listeners.forEach((l) => l());
}

export function getTokens(): TokenPair | null {
  if (cache === undefined) cache = read();
  return cache;
}

export function saveTokens(tokens: TokenPair) {
  cache = tokens;
  try {
    localStorage.setItem(KEY, JSON.stringify(tokens));
  } catch {
    // Storage can be blocked (private mode). The session then lasts until reload.
  }
  notify();
}

export function clearTokens() {
  cache = null;
  try {
    localStorage.removeItem(KEY);
  } catch {
    // Nothing to clear.
  }
  notify();
}

// Returns an unsubscribe function. Also fires when another tab signs in or out.
export function onTokensChange(listener: Listener): () => void {
  listeners.add(listener);
  const onStorage = (e: StorageEvent) => {
    if (e.key !== KEY) return;
    cache = undefined;
    listener();
  };
  window.addEventListener("storage", onStorage);
  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", onStorage);
  };
}
