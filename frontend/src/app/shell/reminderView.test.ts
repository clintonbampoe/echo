import { reminderPhase, reminderView } from "./reminderView";

const base = {
  loaded: true,
  unverified: true,
  sawUnverified: true,
  gone: false,
};

describe("reminderPhase", () => {
  it("draws nothing until the profile has loaded", () => {
    expect(reminderPhase({ ...base, loaded: false })).toBe("hidden");
  });

  it("reminds while the account is unverified", () => {
    expect(reminderPhase(base)).toBe("idle");
  });

  it("thanks the person once, but says nothing to someone who was never unverified", () => {
    const verified = { ...base, unverified: false };
    expect(reminderPhase(verified)).toBe("confirmed");
    expect(reminderPhase({ ...verified, gone: true })).toBe("hidden");
    expect(reminderPhase({ ...verified, sawUnverified: false })).toBe("hidden");
  });
});

describe("reminderView", () => {
  it("asks to verify the account, with the address it will write to", () => {
    const view = reminderView("idle", "a@b.c");
    expect(view.title.long).toBe("Verify your account");
    expect(view.text.long).toBe("We will send a link to a@b.c.");
    expect(view.action?.long).toBe("Verify account");
  });

  it("has no action once verified", () => {
    expect(reminderView("confirmed", "a@b.c").action).toBeUndefined();
  });
});
