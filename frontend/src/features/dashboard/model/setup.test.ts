import { describe, expect, it } from "vitest";
import { factsKnown, nextStep, setupSteps, stepsDone } from "./setup";

const none = {
  members: false,
  attendance: false,
  tithes: false,
  invitations: false,
};

describe("setup steps", () => {
  it("starts with the church done and the first member next", () => {
    const steps = setupSteps(none);
    expect(stepsDone(steps)).toBe(1);
    expect(nextStep(steps)?.key).toBe("member");
  });

  it("moves on to the first step not done, whatever the order of doing", () => {
    const steps = setupSteps({ ...none, members: true, tithes: true });
    expect(stepsDone(steps)).toBe(3);
    expect(nextStep(steps)?.key).toBe("attendance");
  });

  it("has no next step when everything is done", () => {
    const steps = setupSteps({
      members: true,
      attendance: true,
      tithes: true,
      invitations: true,
    });
    expect(stepsDone(steps)).toBe(5);
    expect(nextStep(steps)).toBeUndefined();
  });

  it("is not known until every answer is in", () => {
    expect(factsKnown({ ...none, tithes: undefined })).toBe(false);
    expect(factsKnown(none)).toBe(true);
  });
});
