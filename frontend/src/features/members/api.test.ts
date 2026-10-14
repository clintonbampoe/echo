import { getMemberSummary, listMembers } from "./api";

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

describe("members api", () => {
  it("asks for a page of members with the filters that are set", async () => {
    await listMembers({ name: "ama", status: "Active", region: undefined });
    const url = new URL((fetchMock.mock.calls[0] as [string])[0]);
    expect(url.pathname).toMatch(/\/v1\/members$/);
    expect(url.searchParams.get("name")).toBe("ama");
    expect(url.searchParams.get("status")).toBe("Active");
    expect(url.searchParams.get("pageSize")).toBe("50");
    expect(url.searchParams.has("region")).toBe(false);
    expect(url.searchParams.has("cursor")).toBe(false);
  });

  it("resumes from a cursor and returns the window the server used", async () => {
    const result = await listMembers({}, "abc");
    const url = new URL((fetchMock.mock.calls[0] as [string])[0]);
    expect(url.searchParams.get("cursor")).toBe("abc");
    expect(result.window?.isDefault).toBe(true);
  });

  it("asks for the summary with the same filters as the list", async () => {
    await getMemberSummary({ gender: "Female", from: "2026-01-01" });
    const url = new URL((fetchMock.mock.calls[0] as [string])[0]);
    expect(url.pathname).toMatch(/\/v1\/members\/summary$/);
    expect(url.searchParams.get("gender")).toBe("Female");
    expect(url.searchParams.get("from")).toBe("2026-01-01");
    expect(url.searchParams.has("pageSize")).toBe(false);
  });
});
