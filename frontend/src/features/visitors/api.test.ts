import { getVisitorSummary, listVisitors } from "./api";

const page = {
  data: [],
  hasMore: false,
  next: null,
  window: { from: "2026-01-01", to: "2026-12-31", isDefault: true },
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  fetchMock = vi.fn(
    async () => new Response(JSON.stringify(page), { status: 200 }),
  );
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const urlOf = () => new URL((fetchMock.mock.calls[0] as [string])[0]);

describe("visitors api", () => {
  it("asks for a page of visitors with the filters that are set", async () => {
    await listVisitors({ name: "esi", converted: true, from: undefined });
    const url = urlOf();
    expect(url.pathname).toMatch(/\/v1\/visitors$/);
    expect(url.searchParams.get("name")).toBe("esi");
    expect(url.searchParams.get("converted")).toBe("true");
    expect(url.searchParams.get("pageSize")).toBe("50");
    expect(url.searchParams.has("from")).toBe(false);
  });

  it("leaves converted out for the default list of visitors who are not yet members", async () => {
    await listVisitors({});
    expect(urlOf().searchParams.has("converted")).toBe(false);
  });

  it("asks for the summary with just the time range", async () => {
    await getVisitorSummary({ from: "2025-01-01", to: "2025-12-31" });
    const url = urlOf();
    expect(url.pathname).toMatch(/\/v1\/visitors\/summary$/);
    expect(url.searchParams.get("from")).toBe("2025-01-01");
    expect(url.searchParams.has("pageSize")).toBe(false);
  });
});
