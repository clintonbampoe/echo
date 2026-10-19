import { countLabel, plainList } from "./paged";

describe("countLabel", () => {
  it("is the number when everything is loaded", () => {
    expect(countLabel(9, false)).toBe("9");
  });
  it('says "at least" when there is more to load', () => {
    expect(countLabel(50, true)).toBe("50+");
  });
});

describe("plainList", () => {
  it("shapes a whole list as a list with nothing more to load", () => {
    const retry = vi.fn();
    const list = plainList(["a", "b"], {
      isPending: false,
      isError: true,
      refetch: retry,
    });
    expect(list).toMatchObject({
      rows: ["a", "b"],
      loading: false,
      failed: true,
      hasMore: false,
      loadingMore: false,
    });
    list.retry();
    expect(retry).toHaveBeenCalledOnce();
  });
});
