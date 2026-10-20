import {
  periods,
  previousSpan,
  rangeHint,
  spanOf,
  spokenSpan,
  type PeriodKey,
} from "./periods";

// A Monday: the week began on Sunday 18 October.
const TODAY = "2026-10-19";

describe("spanOf", () => {
  it.each<[PeriodKey, string, string]>([
    ["this-week", "2026-10-18", "2026-10-19"],
    ["last-week", "2026-10-11", "2026-10-17"],
    ["this-month", "2026-10-01", "2026-10-19"],
    ["last-3-months", "2026-07-20", "2026-10-19"],
    ["last-6-months", "2026-04-20", "2026-10-19"],
    ["this-year", "2026-01-01", "2026-10-19"],
  ])("%s runs from %s to %s", (key, from, to) => {
    expect(spanOf(key, TODAY)).toEqual({ from, to });
  });

  it("covers every period the pill offers", () => {
    expect(periods).toHaveLength(6);
  });
});

describe("previousSpan", () => {
  it("is the same number of days, ending the day before", () => {
    expect(previousSpan(spanOf("last-week", TODAY))).toEqual({
      from: "2026-10-04",
      to: "2026-10-10",
    });
    expect(previousSpan(spanOf("this-month", TODAY))).toEqual({
      from: "2026-09-12",
      to: "2026-09-30",
    });
  });
});

describe("writing a span out", () => {
  it("gives the short form for the menu", () => {
    expect(rangeHint(spanOf("last-week", TODAY))).toBe("11 – 17 Oct");
    expect(rangeHint(spanOf("last-3-months", TODAY))).toBe("20 Jul – 19 Oct");
    expect(rangeHint({ from: "2026-10-18", to: "2026-10-18" })).toBe("18 Oct");
  });

  it("gives the long form for a sentence", () => {
    expect(spokenSpan(spanOf("last-week", TODAY))).toBe("11 to 17 October");
    expect(spokenSpan(spanOf("this-year", TODAY))).toBe(
      "1 January to 19 October",
    );
  });
});
