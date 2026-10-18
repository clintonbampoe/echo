import type { EventItem, EventSummary } from "./api";
import { summaryStats, tabCounts } from "./stat";

const s: EventSummary = {
  totalEvents: 24,
  upcomingEvents: 6,
  totalRegistered: 1284,
  totalAttended: 1102,
};
const next = { startDate: "2026-10-17" } as EventItem;

describe("event summary", () => {
  it("shows the four numbers", () => {
    const stats = summaryStats(s, next);
    expect(stats.map((x) => x.value)).toEqual(["24", "6", "1,284", "1,102"]);
    expect(stats[0]?.note).toBe("6 coming up");
    expect(stats[1]?.note).toBe("Next: Sat 17 Oct");
    expect(stats[3]?.note).toBe("86% of those registered");
  });

  it("shows dashes while it loads, and says when nothing is planned", () => {
    expect(summaryStats(undefined, undefined).map((x) => x.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
    expect(summaryStats(s, null)[1]?.note).toBe("Nothing planned");
  });

  it("does not divide by nobody", () => {
    expect(
      summaryStats({ ...s, totalRegistered: 0, totalAttended: 0 }, next)[3]
        ?.note,
    ).toBe("No one registered yet");
  });

  it("counts the two tabs from the summary", () => {
    expect(tabCounts(s)).toEqual({ upcoming: 6, past: 18 });
    expect(tabCounts(undefined)).toEqual({ upcoming: null, past: null });
  });
});
