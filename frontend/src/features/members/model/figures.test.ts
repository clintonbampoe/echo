import type { MemberSummary } from "../api/members";
import { changeLabel, memberFigures, percent } from "./figures";

const summary = (over: Partial<MemberSummary> = {}): MemberSummary => ({
  totalMembers: 60,
  activeMembers: 55,
  femaleCount: 54,
  maleCount: 46,
  averageAge: 38.4,
  ...over,
});

describe("member figures", () => {
  it("never divides by nothing", () => {
    expect(percent(3, 0)).toBe(0);
    expect(percent(54, 100)).toBe(54);
  });

  it("says how much a count grew or shrank, and says nothing without an earlier count", () => {
    expect(changeLabel(60, 46)).toBe("+30%");
    expect(changeLabel(40, 50)).toBe("−20%");
    expect(changeLabel(50, 50)).toBe("No change");
    expect(changeLabel(5, 0)).toBeUndefined();
  });

  it("shows New this year with its change, then women, men and average age", () => {
    const stats = memberFigures(
      "year",
      summary(),
      summary({ totalMembers: 46 }),
    );
    expect(stats.map((s) => [s.label, s.value, s.unit])).toEqual([
      ["New this year", "60", undefined],
      ["Women", "54", "%"],
      ["Men", "46", "%"],
      ["Average age", "38", undefined],
    ]);
    expect(stats[0]?.change).toBe("+30%");
  });

  it("names the range it covers and only compares a year with the one before", () => {
    expect(memberFigures("all", summary(), summary())[0]).toMatchObject({
      label: "All members",
      change: undefined,
    });
    expect(memberFigures("last-year", summary())[0]?.label).toBe(
      "New last year",
    );
  });

  it("shows dashes while the numbers load", () => {
    expect(memberFigures("year").map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
  });
});
