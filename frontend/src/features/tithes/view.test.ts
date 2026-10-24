import {
  chartBars,
  chartSummary,
  titheMonths,
  titheStats,
  yearSlots,
} from "./view";

const rows = [1, 2, 3].map((month) => ({
  month,
  totalCollected: month === 2 ? 0 : 100,
}));

describe("titheMonths", () => {
  it("counts the months so far that had a tithe", () => {
    expect(titheMonths(rows, "2026-03-20")).toEqual({ given: 2, elapsed: 3 });
  });
  it("ignores months that have not come yet", () => {
    expect(
      titheMonths([{ month: 5, totalCollected: 100 }], "2026-03-20").given,
    ).toBe(0);
  });
});

describe("yearSlots", () => {
  it("gives twelve squares: given, not given, and the months still to come", () => {
    const slots = yearSlots(rows, "2026-03-20");
    expect(slots).toHaveLength(12);
    expect(slots.slice(0, 4).map((s) => s.state)).toEqual([
      "on",
      "off",
      "on",
      "future",
    ]);
    expect(slots[0]?.name).toBe("January");
  });
});

describe("tithe page numbers", () => {
  const rows = [
    { year: 2025, month: 11, totalCollected: 12000 },
    { year: 2026, month: 9, totalCollected: 21300 },
    { year: 2026, month: 10, totalCollected: 0 },
  ];

  it("turns the monthly totals into bars with short and full month names", () => {
    expect(chartBars(rows)).toEqual([
      {
        key: "2025-11",
        label: "Nov",
        name: "November 2025",
        tip: "Nov 2025",
        value: 12000,
      },
      {
        key: "2026-09",
        label: "Sep",
        name: "September 2026",
        tip: "Sep 2026",
        value: 21300,
      },
      {
        key: "2026-10",
        label: "Oct",
        name: "October 2026",
        tip: "Oct 2026",
        value: 0,
      },
    ]);
  });

  it("makes the four numbers, with dashes while loading", () => {
    const summary = {
      totalCollected: 18450,
      uniqueTithers: 62,
      mostUsedPaymentMethod: "MobileMoney",
      averagePerMember: 297.58,
    };
    expect(titheStats(summary, 214, "Collected this week")).toEqual([
      {
        label: "Collected",
        value: "18,450",
        unit: "GHS",
        note: "Collected this week",
      },
      {
        label: "Tithers",
        value: "62",
        note: "Out of 214 active members",
      },
      {
        label: "Average tithe",
        value: "297.58",
        unit: "GHS",
        note: "Collected divided by those who gave",
      },
      {
        label: "Most used",
        value: "Mobile money",
        note: "Of the tithes this period",
      },
    ]);
    const loading = titheStats(undefined, undefined, "Collected this week");
    expect(loading.map((x) => x.value)).toEqual(["–", "–", "–", "–"]);
    expect(loading[1]?.note).toBe("");
  });
});

describe("chartSummary", () => {
  it("names the months the bars cover, and which one was highest", () => {
    const bars = chartBars([
      { year: 2025, month: 11, totalCollected: 1 },
      { year: 2026, month: 10, totalCollected: 2 },
    ]);
    expect(chartSummary(bars)).toEqual({
      range: "Nov 2025 – Oct 2026",
      highest: { amount: "GH₵ 2", month: "October" },
    });
  });

  it("leaves out the highest month when nothing was given, and says nothing for an empty chart", () => {
    expect(
      chartSummary(chartBars([{ year: 2026, month: 10, totalCollected: 0 }])),
    ).toEqual({ range: "Oct 2026 – Oct 2026", highest: undefined });
    expect(chartSummary([])).toBeNull();
  });
});
