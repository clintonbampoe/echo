import { ApiError } from "@/lib/api";
import { footnote, isFull, logText, recordProblem, rowState } from "./record";

describe("full events", () => {
  it("is full once every spot is taken, and never when there is no limit", () => {
    expect(isFull({ registrationCount: 60, capacity: 60 })).toBe(true);
    expect(isFull({ registrationCount: 61, capacity: 60 })).toBe(true);
    expect(isFull({ registrationCount: 59, capacity: 60 })).toBe(false);
    expect(isFull({ registrationCount: 500, capacity: null })).toBe(false);
  });
});

describe("where a person stands", () => {
  it("puts those recorded just now first, then those recorded before", () => {
    const before = new Set(["m1", "m2"]);
    expect(rowState("m1", before, [])).toBe("already");
    expect(rowState("m1", before, ["m1"])).toBe("done");
    expect(rowState("m3", before, ["m1"])).toBe("ready");
  });
});

describe("panel text", () => {
  it("counts what was recorded just now", () => {
    expect(logText("register", 0)).toBe("Nothing recorded yet");
    expect(logText("register", 2)).toBe("2 registered just now");
    expect(logText("check-in", 3)).toBe("3 checked in just now");
  });

  it("explains walk-ins when checking in, and that check-in still works when registration is closed", () => {
    expect(footnote("check-in", false, 4)).toBe(
      "Not registered? You can still check them in. They show as a walk-in.",
    );
    expect(footnote("register", true, 4)).toBe(
      "Check in still works while registration is closed.",
    );
    expect(footnote("register", false, 4)).toBe(
      "Showing the top 4 matches. Keep typing to narrow it down.",
    );
    expect(footnote("register", false, 1)).toBe("Showing 1 match.");
    expect(footnote("register", false, undefined)).toBeNull();
  });
});

describe("when a recording is refused", () => {
  it("says registration is closed when the event filled up", () => {
    const full = new ApiError(409, "EVENT_FULL", "Event is full");
    expect(recordProblem("register", "Kofi Mensah", full).title).toBe(
      "Registration is closed",
    );
  });

  it("says someone else got there first on any other conflict", () => {
    const same = new ApiError(409, "EVENT_REGISTRATION_DUPLICATE", "Already");
    expect(recordProblem("register", "Kofi Mensah", same).title).toBe(
      "Kofi Mensah is already registered",
    );
    expect(recordProblem("check-in", "Kofi Mensah", same).title).toBe(
      "Kofi Mensah is already checked in",
    );
  });

  it("asks to try again for anything else", () => {
    expect(
      recordProblem("check-in", "Kofi Mensah", new Error("offline")).title,
    ).toBe("We could not check in Kofi Mensah");
  });
});
