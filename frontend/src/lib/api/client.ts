import { config } from "@/lib/config";
import {
  clearTokens,
  getTokens,
  saveTokens,
  type TokenPair,
} from "@/lib/tokens";
import { networkError, parseProblem } from "./errors";

// A list value is sent as the same key repeated (`ids=a&ids=b`), which is how the API reads a list.
type Query = Record<
  string,
  string | number | boolean | undefined | null | readonly string[]
>;

type Options = {
  body?: unknown;
  query?: Query;
  signal?: AbortSignal;
  // false for calls that must not carry a token or trigger a refresh (login, forgot password).
  auth?: boolean;
};

const REFRESH_PATH = "/v1/auth/sessions/refresh";

function buildUrl(path: string, query?: Query): string {
  const url = new URL(config.apiUrl + path, window.location.origin);
  for (const [key, value] of Object.entries(query ?? {})) {
    if (Array.isArray(value)) {
      for (const item of value) url.searchParams.append(key, item);
    } else if (value !== undefined && value !== null && value !== "") {
      url.searchParams.set(key, String(value));
    }
  }
  return url.toString();
}

async function send(
  method: string,
  path: string,
  opts: Options,
  token: string | null,
): Promise<Response> {
  const headers: Record<string, string> = { Accept: "application/json" };
  if (opts.body !== undefined) headers["Content-Type"] = "application/json";
  if (token) headers.Authorization = `Bearer ${token}`;
  try {
    return await fetch(buildUrl(path, opts.query), {
      method,
      headers,
      body: opts.body === undefined ? undefined : JSON.stringify(opts.body),
      signal: opts.signal,
    });
  } catch (e) {
    if (e instanceof DOMException && e.name === "AbortError") throw e;
    throw networkError();
  }
}

// If several requests get a 401 at once, they share one refresh. Refresh tokens are
// single-use, so a second refresh with the same token would sign the user out.
let refreshing: Promise<boolean> | null = null;

function refreshSession(): Promise<boolean> {
  refreshing ??= doRefresh().finally(() => {
    refreshing = null;
  });
  return refreshing;
}

async function doRefresh(): Promise<boolean> {
  const current = getTokens();
  if (!current) return false;
  try {
    const res = await send(
      "POST",
      REFRESH_PATH,
      { body: { refreshToken: current.refreshToken } },
      null,
    );
    if (!res.ok) {
      clearTokens();
      return false;
    }
    saveTokens((await res.json()) as TokenPair);
    return true;
  } catch {
    // Offline is not the same as signed out: keep the tokens and let the caller see the failure.
    return false;
  }
}

async function readBody(res: Response): Promise<unknown> {
  const text = await res.text();
  if (!text) return undefined;
  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}

function accessToken(): string | null {
  return getTokens()?.accessToken ?? null;
}

// A 401 on an authenticated call means the access token expired: refresh once and try again.
async function sendWithRefresh(
  method: string,
  path: string,
  opts: Options,
): Promise<Response> {
  if (opts.auth === false) return send(method, path, opts, null);

  const res = await send(method, path, opts, accessToken());
  if (res.status !== 401 || !getTokens()) return res;
  if (!(await refreshSession())) return res;
  return send(method, path, opts, accessToken());
}

async function request<T>(
  method: string,
  path: string,
  opts: Options = {},
): Promise<T> {
  const res = await sendWithRefresh(method, path, opts);
  const body = await readBody(res);
  if (!res.ok) throw parseProblem(res.status, body);
  return body as T;
}

export const api = {
  get: <T>(path: string, opts?: Omit<Options, "body">) =>
    request<T>("GET", path, opts),
  post: <T = void>(
    path: string,
    body?: unknown,
    opts?: Omit<Options, "body">,
  ) => request<T>("POST", path, { ...opts, body }),
  put: <T = void>(path: string, body?: unknown, opts?: Omit<Options, "body">) =>
    request<T>("PUT", path, { ...opts, body }),
  patch: <T = void>(
    path: string,
    body?: unknown,
    opts?: Omit<Options, "body">,
  ) => request<T>("PATCH", path, { ...opts, body }),
  delete: <T = void>(path: string, opts?: Omit<Options, "body">) =>
    request<T>("DELETE", path, opts),
};
