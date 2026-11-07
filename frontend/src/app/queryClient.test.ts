import { KEEP_MS, queryClient } from "./queryClient";

describe("queryClient", () => {
  it("keeps data a while after the page that used it has gone, for Back", () => {
    expect(queryClient.getDefaultOptions().queries?.gcTime).toBe(KEEP_MS);
    expect(KEEP_MS).toBeGreaterThan(5 * 60_000);
  });
});
