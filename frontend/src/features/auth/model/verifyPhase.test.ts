import { beforeEach, describe, expect, it } from "vitest";
import { markSent, sentRecently, verifyPhase } from "./verifyPhase";

describe("verifyPhase", () => {
  const base = { verified: false, send: "sent", slow: false } as const;
  it("is verified above everything else", () => {
    expect(verifyPhase({ ...base, verified: true, send: "failed" })).toBe(
      "verified",
    );
  });
  it("follows the send, then how long the person has waited", () => {
    expect(verifyPhase({ ...base, send: "sending" })).toBe("sending");
    expect(verifyPhase({ ...base, send: "failed" })).toBe("failed");
    expect(verifyPhase(base)).toBe("waiting");
    expect(verifyPhase({ ...base, slow: true })).toBe("slow");
  });
});

describe("sentRecently", () => {
  beforeEach(() => sessionStorage.clear());
  it("remembers a send for half a minute", () => {
    expect(sentRecently("a@b.c", 1_000)).toBe(false);
    markSent("a@b.c", 1_000);
    expect(sentRecently("a@b.c", 20_000)).toBe(true);
    expect(sentRecently("a@b.c", 40_000)).toBe(false);
    expect(sentRecently("other@b.c", 2_000)).toBe(false);
  });
});
