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

  // The email link is spent by reading it once, so a second read would wrongly report it expired. The
  // profile and congregation are not what the server drops on a write, and each has its own update.
  it("leaves sign-in and account queries alone after a write", async () => {
    queryClient.setQueryData(["auth", "verify", "abc"], true);
    queryClient.setQueryData(["account", "me"], { name: "Ama" });

    await new MutationObserver(queryClient, {
      mutationFn: () => Promise.resolve("saved"),
    }).mutate();

    expect(queryClient.getQueryState(["auth", "verify", "abc"])?.isInvalidated).toBe(false);
    expect(queryClient.getQueryState(["account", "me"])?.isInvalidated).toBe(false);
  });
});
