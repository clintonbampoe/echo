import type { AttendanceSummary } from "./api";
import {
  dayMonth,
  dayName,
  longDay,
  nearestSessionDay,
  needsNewAnchor,
  presenceStats,
  readDay,
  sessionTitle,
  shortDay,
  stripDays,
  stripRange,
  timeRange,
} from "./day";

const summary = (over: Partial<AttendanceSummary> = {}): AttendanceSummary => ({
  totalPresent: 9,
  membersPresent: 8,
  visitorsPresent: 1,
  firstTimeVisitors: 1,
  ...over,
});

describe("the day strip", () => {
  it("asks for the sessions three weeks either side of the picked day", () => {
    expect(stripRange("2026-10-14")).toEqual({
      from: "2026-09-23",
      to: "2026-11-04",
    });
  });

  it("marks today, counts the sessions on each day and names the month on its first", () => {
    const days = stripDays("2026-10-14", "2026-10-14", [
      { date: "2026-10-11" },
      { date: "2026-10-11" },
      { date: "2026-10-14" },
    ]);
    expect(days).toHaveLength(91);
    expect(days[0]).toMatchObject({
      day: "2026-08-30",
      weekday: "Sun",
      date: 30,
      marks: 0,
      today: false,
    });
    expect(days.find((d) => d.day === "2026-10-11")?.marks).toBe(2);
    expect(days.filter((d) => d.today).map((d) => d.day)).toEqual([
      "2026-10-14",
    ]);
    expect(days.find((d) => d.today)?.tag).toBe("Today");
    expect(days.find((d) => d.day === "2026-11-01")?.tag).toBe("Nov");
    expect(days.find((d) => d.day === "2026-10-12")?.tag).toBeUndefined();
  });

  it("moves its anchor only once the picked day is well away from it", () => {
    expect(needsNewAnchor("2026-10-14", "2026-11-05")).toBe(false);
    expect(needsNewAnchor("2026-10-14", "2026-09-20")).toBe(false);
    expect(needsNewAnchor("2026-10-14", "2026-11-20")).toBe(true);
    expect(needsNewAnchor("2026-10-14", "2026-08-01")).toBe(true);
  });
});

describe("writing a day", () => {
  it("writes the long, short and year-less forms", () => {
    expect(longDay("2026-10-11")).toBe("Sunday, 11 October 2026");
    expect(dayName("2026-10-11")).toBe("Sunday, 11 October");
    expect(shortDay("2026-10-14")).toBe("Wed 14 Oct");
  });
});

describe("a session", () => {
  it("is called by its name, or by its type", () => {
    expect(
      sessionTitle({
        name: "Christmas Eve Service",
        sessionLabelName: "Sunday Service",
      }),
    ).toBe("Christmas Eve Service");
    expect(
      sessionTitle({ name: null, sessionLabelName: "Sunday Service" }),
    ).toBe("Sunday Service");
  });

  it("writes when it runs", () => {
    expect(timeRange({ startsAt: "08:00:00", endsAt: "10:30:00" })).toBe(
      "8:00 am to 10:30 am",
    );
    expect(timeRange({ startsAt: "18:00:00", endsAt: null })).toBe("6:00 pm");
    expect(timeRange({ startsAt: null, endsAt: "10:30:00" })).toBe("");
  });
});

describe("presence numbers", () => {
  it("splits members and visitors so the shares add to 100", () => {
    const stats = presenceStats(
      summary({ totalPresent: 3, membersPresent: 2, visitorsPresent: 1 }),
      "Across 2 sessions",
      "No one yet",
    );
    expect(stats.map((s) => [s.label, s.value, s.note])).toEqual([
      ["Present", "3", "Across 2 sessions"],
      ["Members", "2", "67% of those present"],
      ["Visitors", "1", "33% of those present"],
      ["First-time visitors", "1", "Came for the first time"],
    ]);
  });

  it("says nobody is here yet instead of 0%", () => {
    const stats = presenceStats(
      summary({
        totalPresent: 0,
        membersPresent: 0,
        visitorsPresent: 0,
        firstTimeVisitors: 0,
      }),
      "x",
      "Not started",
    );
    expect(stats.map((s) => s.note).slice(0, 3)).toEqual([
      "Not started",
      "Not started",
      "Not started",
    ]);
  });

  it("shows dashes while loading", () => {
    expect(
      presenceStats(undefined, "x", "y").every((s) => s.value === "–"),
    ).toBe(true);
  });
});

describe("the day in the address", () => {
  it("uses a real day, and today for anything else", () => {
    expect(readDay("2026-10-11", "2026-10-14")).toBe("2026-10-11");
    expect(readDay("2026-02-30", "2026-10-14")).toBe("2026-10-14");
    expect(readDay("soon", "2026-10-14")).toBe("2026-10-14");
    expect(readDay(null, "2026-10-14")).toBe("2026-10-14");
  });

  it("writes a day and month", () => {
    expect(dayMonth("2026-10-11")).toBe("11 Oct");
  });
});

describe("the nearest day with a session", () => {
  const sessions = [
    { date: "2026-10-04" },
    { date: "2026-10-11" },
    { date: "2026-10-18" },
  ];
  it("prefers the next one, then the one before", () => {
    expect(nearestSessionDay("2026-10-12", sessions)).toBe("2026-10-18");
    expect(nearestSessionDay("2026-10-19", sessions)).toBe("2026-10-18");
    expect(nearestSessionDay("2026-10-12", [])).toBeNull();
  });
});
