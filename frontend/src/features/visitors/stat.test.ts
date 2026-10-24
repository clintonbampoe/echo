import { firstVisitStat, becameStat, daysSinceStat, visitsStat } from "./stat";

describe("visitor stats", () => {
  it("counts visits and says since when", () => {
    expect(visitsStat({ visitCount: 3 })).toMatchObject({
      value: "3",
      note: "As a visitor",
    });
    expect(firstVisitStat({ createdAt: "2026-09-07T10:00:00Z" })).toMatchObject(
      {
        label: "First visit",
        note: "First recorded",
      },
    );
  });

  it("days since: a dash with no visits, a nudge after two weeks", () => {
    expect(daysSinceStat({}, "2026-10-14")).toMatchObject({
      value: "–",
      note: "No visits yet",
    });
    expect(
      daysSinceStat({ lastVisitedAt: "2026-10-11T09:00:00Z" }, "2026-10-14"),
    ).toMatchObject({ value: "3", note: "Seen recently" });
    expect(
      daysSinceStat({ lastVisitedAt: "2026-09-20T09:00:00Z" }, "2026-10-14"),
    ).toMatchObject({ value: "24", note: "Time to follow up" });
  });

  it("became member: the date and how many visits it took", () => {
    expect(
      becameStat({ convertedAt: "2026-10-12T09:00:00Z", visitCount: 1 }).note,
    ).toBe("After 1 visit");
    expect(
      becameStat({ convertedAt: "2026-10-12T09:00:00Z", visitCount: 4 }).note,
    ).toBe("After 4 visits");
  });
});
