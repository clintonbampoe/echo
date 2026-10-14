import { ageOn, daysBetween, monthsBefore, thisYear } from "./dates";

describe("monthsBefore", () => {
  it("goes back whole months, across a year", () => {
    expect(monthsBefore("2026-10-14", 3)).toBe("2026-07-14");
    expect(monthsBefore("2026-02-10", 3)).toBe("2025-11-10");
    expect(monthsBefore("2026-10-14", 12)).toBe("2025-10-14");
  });
  it("stops at the end of a shorter month", () => {
    expect(monthsBefore("2026-05-31", 3)).toBe("2026-02-28");
    expect(monthsBefore("2028-05-31", 3)).toBe("2028-02-29");
  });
});

describe("thisYear", () => {
  it("runs from 1 January to today", () => {
    expect(thisYear("2026-10-14")).toEqual({
      year: 2026,
      from: "2026-01-01",
      to: "2026-10-14",
    });
  });
});

describe("ageOn", () => {
  it("goes up on the birthday, not before", () => {
    expect(ageOn("1988-02-14", "2026-02-13")).toBe(37);
    expect(ageOn("1988-02-14", "2026-02-14")).toBe(38);
  });
  it("ignores a time part", () => {
    expect(ageOn("1988-02-14T00:00:00", "2026-10-14")).toBe(38);
  });
});

describe("daysBetween", () => {
  it("counts whole days, across months and years", () => {
    expect(daysBetween("2026-10-01", "2026-10-14")).toBe(13);
    expect(daysBetween("2025-12-30", "2026-01-02")).toBe(3);
  });
  it("ignores a time part and never goes below zero", () => {
    expect(daysBetween("2026-10-14T18:00:00Z", "2026-10-14")).toBe(0);
    expect(daysBetween("2026-10-20", "2026-10-14")).toBe(0);
  });
});
