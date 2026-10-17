import {
  countActive,
  isNarrowed,
  recordFilters,
  toCheckInQuery,
  toSessionFilters,
  windowDates,
} from "./records";

const today = "2026-10-16";
const empty = recordFilters.empty;

describe("windowDates", () => {
  it("turns each window into days", () => {
    expect(windowDates({ ...empty, window: "week" }, today)).toEqual({
      from: "2026-10-11",
      to: today,
    });
    expect(windowDates(empty, today)).toEqual({
      from: "2026-10-01",
      to: today,
    });
    expect(windowDates({ ...empty, window: "year" }, today)).toEqual({
      from: "2026-01-01",
      to: today,
    });
    expect(windowDates({ ...empty, window: "last-year" }, today)).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
  });

  it("uses a custom range, leaves a missing side open and turns a backwards range round", () => {
    const custom = { ...empty, window: "custom" };
    expect(
      windowDates({ ...custom, from: "2026-03-01", to: "2026-04-01" }, today),
    ).toEqual({ from: "2026-03-01", to: "2026-04-01" });
    expect(windowDates({ ...custom, from: "2026-03-01" }, today)).toEqual({
      from: "2026-03-01",
      to: today,
    });
    expect(
      windowDates({ ...custom, from: "2026-04-01", to: "2026-03-01" }, today),
    ).toEqual({ from: "2026-03-01", to: "2026-04-01" });
  });
});

describe("what the API is asked", () => {
  it("sends the days, the type and the search for sessions, and the kind for people", () => {
    const f = {
      ...empty,
      name: " Ama ",
      type: "3",
      status: "Completed",
      kind: "Visitor",
    };
    expect(toSessionFilters(f, today)).toEqual({
      sessionLabelId: 3,
      status: "Completed",
      name: "Ama",
      from: "2026-10-01",
      to: today,
    });
    expect(toCheckInQuery(f, today)).toEqual({
      sessionLabelId: 3,
      kind: "Visitor",
      name: "Ama",
      from: "2026-10-01",
      to: today,
    });
  });

  it("ignores values that are not a status or a kind", () => {
    expect(
      toSessionFilters({ ...empty, status: "x" }, today).status,
    ).toBeUndefined();
    expect(toCheckInQuery({ ...empty, kind: "x" }, today).kind).toBeUndefined();
  });
});

describe("what is narrowing the list", () => {
  it("counts only the filters that apply to the view", () => {
    const f = {
      ...empty,
      type: "3",
      status: "Completed",
      kind: "Visitor",
      window: "year",
    };
    expect(countActive(f, false)).toBe(3);
    expect(countActive(f, true)).toBe(3);
    expect(countActive({ ...empty, status: "Completed" }, true)).toBe(0);
    expect(isNarrowed({ ...empty, status: "Completed" }, true)).toBe(false);
    expect(isNarrowed({ ...empty, status: "Completed" }, false)).toBe(true);
    expect(isNarrowed(empty, false)).toBe(false);
  });
});
