import type { EventItem, EventSummary } from "./api";
import { summaryStats } from "./stat";

const s: EventSummary = {
  totalEvents: 24,
  upcomingEvents: 6,
  totalRegistered: 1284,
  totalAttended: 1102,
};
const next = { startDate: "2026-10-17" } as EventItem;

describe("event summary", () => {
  it("shows what is coming up over the upcoming events", () => {
    const stats = summaryStats(s, next, "upcoming");
    expect(stats.map((x) => [x.label, x.value])).toEqual([
      ["Upcoming", "6"],
      ["Registered", "1,284"],
      ["Events", "24"],
    ]);
    expect(stats[0]?.note).toBe("Next: Sat 17 Oct");
  });

  it("shows what was held over the past ones: events, check-ins and the average", () => {
    const stats = summaryStats(s, next, "past");
    expect(stats.map((x) => [x.label, x.value])).toEqual([
      ["Events held", "18"],
      ["Check-ins", "1,102"],
      ["Average", "61"],
    ]);
    expect(stats[1]?.note).toBe("86% of those registered");
  });

  it("shows dashes while it loads, and says when nothing is planned", () => {
    expect(
      summaryStats(undefined, undefined, "upcoming").map((x) => x.value),
    ).toEqual(["–", "–", "–"]);
    expect(
      summaryStats(undefined, undefined, "past").map((x) => x.value),
    ).toEqual(["–", "–", "–"]);
    expect(summaryStats(s, null, "upcoming")[0]?.note).toBe("Nothing planned");
  });

  it("does not divide by nobody", () => {
    const none = { ...s, totalRegistered: 0, totalAttended: 0 };
    expect(summaryStats(none, next, "past")[1]?.note).toBe(
      "No one registered yet",
    );
    expect(
      summaryStats({ ...none, totalEvents: 6 }, next, "past")[2]?.value,
    ).toBe("0");
  });
});
