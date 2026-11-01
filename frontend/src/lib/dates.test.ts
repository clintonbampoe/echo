import {
  ageOn,
  daysAfter,
  daysBetween,
  isDay,
  monthGrid,
  monthLength,
  monthStart,
  monthsAfter,
  monthsBefore,
  nowTime,
  spokenDay,
  thisYear,
  weekStart,
} from "./dates";

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
  it("runs from 1 January to today, and counts the months it spans", () => {
    expect(thisYear("2026-10-14")).toEqual({
      year: 2026,
      from: "2026-01-01",
      to: "2026-10-14",
      months: 10,
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

describe("daysAfter", () => {
  it("adds days across months and years", () => {
    expect(daysAfter("2026-10-14", 60)).toBe("2026-12-13");
    expect(daysAfter("2026-12-30", 3)).toBe("2027-01-02");
    expect(daysAfter("2028-02-28", 1)).toBe("2028-02-29");
  });
});

describe("weekStart and monthStart", () => {
  it("finds the Sunday a week starts on, and the first of the month", () => {
    expect(weekStart("2026-10-14")).toBe("2026-10-11");
    expect(weekStart("2026-10-11")).toBe("2026-10-11");
    expect(weekStart("2026-10-03")).toBe("2026-09-27");
    expect(monthStart("2026-10-14")).toBe("2026-10-01");
  });
});

describe("calendar helpers", () => {
  it("knows a real day from a made-up one", () => {
    expect(isDay("2026-02-28")).toBe(true);
    expect(isDay("2026-02-30")).toBe(false);
    expect(isDay("26-2-3")).toBe(false);
  });

  it("knows how long a month is, in a leap year too", () => {
    expect(monthLength(2026, 2)).toBe(28);
    expect(monthLength(2028, 2)).toBe(29);
    expect(monthLength(2026, 10)).toBe(31);
  });

  it("moves a month across a year", () => {
    expect(monthsAfter("2026-11", 3)).toBe("2027-02");
    expect(monthsAfter("2026-01", -1)).toBe("2025-12");
  });

  it("writes a day out in full", () => {
    expect(spokenDay("2026-10-11")).toBe("Sunday 11 October 2026");
  });

  it("lays a month out as 6 Sunday-first weeks", () => {
    const grid = monthGrid("2026-10");
    expect(grid).toHaveLength(42);
    // 1 October 2026 is a Thursday, so the grid starts on the Sunday before it.
    expect(grid[0]).toBe("2026-09-27");
    expect(grid[4]).toBe("2026-10-01");
    expect(grid[41]).toBe("2026-11-07");
  });
});

describe("nowTime", () => {
  it("gives the time on the device as hours, minutes and no seconds", () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 9, 17, 6, 5, 42));
    expect(nowTime()).toBe("06:05:00");
    vi.useRealTimers();
  });
});
