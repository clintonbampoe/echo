import { describe, expect, it } from "vitest";
import {
  factsKnown,
  nextStep,
  nothingYet,
  setupState,
  setupSteps,
  stepLine,
  stepsDone,
  stepsLeft,
} from "./setup";

const none = {
  members: false,
  attendance: false,
  tithes: false,
  invitations: false,
};

describe("setup steps", () => {
  it("starts with the church done and the first member next", () => {
    const steps = setupSteps(none, "Grace Chapel");
    expect(stepsDone(steps)).toBe(1);
    expect(nextStep(steps)?.key).toBe("member");
    expect(steps[0]?.text).toBe(
      "Grace Chapel has its default categories and session types.",
    );
  });

  it("moves on to the first step not done, whatever the order of doing", () => {
    const steps = setupSteps({ ...none, members: true, tithes: true }, "G");
    expect(stepsDone(steps)).toBe(3);
    expect(nextStep(steps)?.key).toBe("attendance");
  });

  it("has no next step when everything is done", () => {
    const all = {
      members: true,
      attendance: true,
      tithes: true,
      invitations: true,
    };
    const steps = setupSteps(all, "G");
    expect(stepsDone(steps)).toBe(5);
    expect(nextStep(steps)).toBeUndefined();
  });

  it("is not known until every answer is in", () => {
    expect(factsKnown({ ...none, tithes: undefined })).toBe(false);
    expect(factsKnown(none)).toBe(true);
  });

  it("calls a church with no members, attendance or tithes empty", () => {
    expect(nothingYet(none)).toBe(true);
    expect(nothingYet({ ...none, invitations: true })).toBe(true);
    expect(nothingYet({ ...none, members: true })).toBe(false);
    expect(nothingYet({ ...none, tithes: undefined })).toBe(false);
  });

  it("counts what is left in words", () => {
    expect(stepsLeft(4)).toBe("Four more steps");
    expect(stepsLeft(1)).toBe("One more step");
  });
});

describe("step line", () => {
  it("says what a step is for, and keeps a done step's line only when it stands alone", () => {
    expect(stepLine("next", "Do it", false, false)).toBe("Do it");
    expect(stepLine("done", "Done it", false, false)).toBeUndefined();
    expect(stepLine("done", "Done it", false, true)).toBe("Done it");
  });
  it("says where each stands in the slim rows", () => {
    expect(stepLine("done", "x", true, false)).toBe("Done");
    expect(stepLine("next", "x", true, false)).toBe("Next step");
    expect(stepLine("todo", "x", true, false)).toBeUndefined();
  });
  it("is off for a non-admin or a hidden card, loading until ready", () => {
    expect(setupState(false, false, true)).toBe("off");
    expect(setupState(true, true, true)).toBe("off");
    expect(setupState(true, false, false)).toBe("loading");
    expect(setupState(true, false, true)).toBe("on");
  });
});
