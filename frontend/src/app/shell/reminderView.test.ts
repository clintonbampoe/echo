import { reminderPhase, reminderView } from "./reminderView";

const base = {
  loaded: true,
  unverified: true,
  sawUnverified: true,
  gone: false,
  sent: false,
  failed: false,
};

describe("reminderPhase", () => {
  it("draws nothing until the profile has loaded", () => {
    expect(reminderPhase({ ...base, loaded: false })).toBe("hidden");
  });

  it("follows an unverified address from idle to sent, and to failed on an error", () => {
    expect(reminderPhase(base)).toBe("idle");
    expect(reminderPhase({ ...base, sent: true })).toBe("sent");
    expect(reminderPhase({ ...base, sent: true, failed: true })).toBe("failed");
  });

  it("thanks the person once, but says nothing to someone who was never unverified", () => {
    const verified = { ...base, unverified: false };
    expect(reminderPhase(verified)).toBe("confirmed");
    expect(reminderPhase({ ...verified, gone: true })).toBe("hidden");
    expect(reminderPhase({ ...verified, sawUnverified: false })).toBe("hidden");
  });
});

describe("reminderView", () => {
  it("counts the wait down as minutes and seconds", () => {
    expect(reminderView("sent", "a@b.c", 42).action?.long).toBe(
      "Resend in 0:42",
    );
    expect(reminderView("sent", "a@b.c", 0).action?.disabled).toBeFalsy();
  });
});
