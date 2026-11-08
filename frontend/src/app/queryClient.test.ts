import { MutationObserver } from "@tanstack/react-query";
import { KEEP_MS, queryClient } from "./queryClient";

describe("queryClient", () => {
  it("keeps data a while after the page that used it has gone, for Back", () => {
    expect(queryClient.getDefaultOptions().queries?.gcTime).toBe(KEEP_MS);
    expect(KEEP_MS).toBeGreaterThan(5 * 60_000);
  });

  // The server drops everything it kept for a congregation when anything in it is written, so a list
  // elsewhere in the app must not keep showing what it held before the write.
  it("marks every query stale once any write succeeds", async () => {
    queryClient.setQueryData(["members"], [1]);
    queryClient.setQueryData(["tithes"], [2]);

    await new MutationObserver(queryClient, {
      mutationFn: () => Promise.resolve("saved"),
    }).mutate();

    expect(queryClient.getQueryState(["members"])?.isInvalidated).toBe(true);
    expect(queryClient.getQueryState(["tithes"])?.isInvalidated).toBe(true);
  });

  it("leaves queries alone when a write fails", async () => {
    queryClient.setQueryData(["members"], [1]);

    await new MutationObserver(queryClient, {
      mutationFn: () => Promise.reject(new Error("nope")),
    })
      .mutate()
      .catch(() => undefined);

    expect(queryClient.getQueryState(["members"])?.isInvalidated).toBe(false);
  });
});
