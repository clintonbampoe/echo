import type { TransactionMonth, TransactionSummary } from "./api";
import {
  chartBars,
  financeStats,
  signedMoney,
  transactionAmount,
} from "./view";

const months: TransactionMonth[] = [
  { year: 2026, month: 9, totalIncome: 4000, totalExpenses: 1500, net: 2500 },
  { year: 2026, month: 10, totalIncome: 6850, totalExpenses: 2140, net: 4710 },
];

const summary: TransactionSummary = {
  totalIncome: 6850,
  totalExpenses: 2140,
  net: 4710,
  mostActiveCategory: "Offerings",
  byCategory: [],
};

describe("the signed amount", () => {
  it("marks money in with a plus and money out with a minus sign", () => {
    expect(transactionAmount({ amount: 6850, transactionType: "Income" })).toBe(
      "+GH₵ 6,850",
    );
    expect(
      transactionAmount({ amount: 1240, transactionType: "Expense" }),
    ).toBe("−GH₵ 1,240");
  });

  it("shows a net loss as a minus, never as a double sign", () => {
    expect(signedMoney(-900, "Expense")).toBe("−GH₵ 900");
  });
});

describe("the four numbers", () => {
  it("names the period under income and expenses, and the busiest category", () => {
    const stats = financeStats(summary, "This week");
    expect(stats.map((s) => s.value)).toEqual([
      "6,850",
      "2,140",
      "+4,710",
      "Offerings",
    ]);
    expect(stats[0]?.note).toBe("This week");
    expect(stats[3]?.note).toBe("By number of transactions");
  });

  it("waits with dashes until the numbers arrive", () => {
    expect(financeStats(undefined, "This week").map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
  });
});

describe("the chart", () => {
  it("gives every month both totals, income first", () => {
    const bars = chartBars(months);
    expect(bars[1]).toMatchObject({
      key: "2026-10",
      label: "Oct",
      tip: "Oct 2026",
      value: 6850,
      text: "GH₵ 6,850",
    });
    expect(bars[1]?.compare).toEqual({ value: 2140, text: "GH₵ 2,140" });
  });

  it("draws nothing when there are no months", () => {
    expect(chartBars([])).toEqual([]);
  });
});
