import { api } from "./client";
import { isApiError } from "./errors";
import { getTokens, saveTokens, type TokenPair } from "@/lib/tokens";

const pair = (n: string): TokenPair => ({
  accessToken: `access-${n}`,
  accessTokenExpiresAt: "2099-01-01T00:00:00Z",
  refreshToken: `refresh-${n}`,
  refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
});

const json = (status: number, body?: unknown) =>
  new Response(body === undefined ? null : JSON.stringify(body), { status });

let fetchMock: ReturnType<typeof vi.fn>;

beforeEach(() => {
  fetchMock = vi.fn();
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

describe("api client", () => {
  it("sends a list as the same key repeated, and nothing for an empty one", async () => {
    saveTokens(pair("1"));
    fetchMock.mockResolvedValueOnce(json(200, []));

    await api.get("/v1/things", { query: { ids: ["a", "b"], none: [] } });

    const [url] = fetchMock.mock.calls[0] as [string];
    expect(url).toContain("/v1/things?ids=a&ids=b");
    expect(url).not.toContain("none");
  });

  it("sends the bearer token and builds the query string, skipping empty values", async () => {
    saveTokens(pair("1"));
    fetchMock.mockResolvedValueOnce(json(200, { ok: true }));

    await api.get("/v1/members", {
      query: { search: "ama", cursor: undefined, limit: 50 },
    });

    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/v1/members?search=ama&limit=50");
    expect((init.headers as Record<string, string>).Authorization).toBe(
      "Bearer access-1",
    );
    expect(
      (init.headers as Record<string, string>)["Content-Type"],
    ).toBeUndefined();
  });

  it("returns undefined for an empty 204 response", async () => {
    fetchMock.mockResolvedValueOnce(json(204));
    await expect(
      api.post("/v1/x", { a: 1 }, { auth: false }),
    ).resolves.toBeUndefined();
  });

  it("throws an ApiError with the errorCode for a failed request", async () => {
    fetchMock.mockResolvedValueOnce(
      json(403, { errorCode: "EMAIL_NOT_VERIFIED", detail: "Verify first." }),
    );
    const err = await api
      .post("/v1/auth/sessions/login", {}, { auth: false })
      .catch((e: unknown) => e);
    expect(isApiError(err)).toBe(true);
    expect(err).toMatchObject({ status: 403, errorCode: "EMAIL_NOT_VERIFIED" });
  });

  it("reports a network failure as NETWORK", async () => {
    fetchMock.mockRejectedValueOnce(new TypeError("failed"));
    const err = await api
      .get("/v1/x", { auth: false })
      .catch((e: unknown) => e);
    expect(err).toMatchObject({ status: 0, errorCode: "NETWORK" });
  });

  it("refreshes once on 401, saves the new pair and retries the request", async () => {
    saveTokens(pair("1"));
    fetchMock
      .mockResolvedValueOnce(json(401, { errorCode: "INVALID_TOKEN" }))
      .mockResolvedValueOnce(json(200, pair("2")))
      .mockResolvedValueOnce(json(200, { name: "ok" }));

    await expect(api.get("/v1/members")).resolves.toEqual({ name: "ok" });

    const retry = fetchMock.mock.calls[2] as [string, RequestInit];
    expect((retry[1].headers as Record<string, string>).Authorization).toBe(
      "Bearer access-2",
    );
    expect(getTokens()?.refreshToken).toBe("refresh-2");
  });

  it("shares one refresh between simultaneous 401s", async () => {
    saveTokens(pair("1"));
    fetchMock.mockImplementation((url: string) => {
      if (url.endsWith("/refresh"))
        return Promise.resolve(json(200, pair("2")));
      const authed = fetchMock.mock.calls.some(([u]) =>
        (u as string).endsWith("/refresh"),
      );
      return Promise.resolve(authed ? json(200, { ok: 1 }) : json(401));
    });

    await Promise.all([api.get("/v1/a"), api.get("/v1/b")]);

    const refreshes = fetchMock.mock.calls.filter(([u]) =>
      (u as string).endsWith("/refresh"),
    );
    expect(refreshes).toHaveLength(1);
  });

  it("clears the session when the refresh is rejected", async () => {
    saveTokens(pair("1"));
    fetchMock
      .mockResolvedValueOnce(json(401))
      .mockResolvedValueOnce(json(401, { errorCode: "INVALID_TOKEN" }));

    await expect(api.get("/v1/members")).rejects.toMatchObject({ status: 401 });
    expect(getTokens()).toBeNull();
  });

  it("does not attach a token or try to refresh when auth is false", async () => {
    saveTokens(pair("1"));
    fetchMock.mockResolvedValueOnce(
      json(401, { errorCode: "INVALID_CREDENTIALS" }),
    );

    await expect(
      api.post("/v1/auth/sessions/login", {}, { auth: false }),
    ).rejects.toMatchObject({
      errorCode: "INVALID_CREDENTIALS",
    });
    expect(fetchMock).toHaveBeenCalledTimes(1);
    const init = fetchMock.mock.calls[0]?.[1] as RequestInit;
    expect(
      (init.headers as Record<string, string>).Authorization,
    ).toBeUndefined();
  });
});
