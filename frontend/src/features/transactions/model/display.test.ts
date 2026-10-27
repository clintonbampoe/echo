import type { TransactionMonth, TransactionSummary } from "../api/transactions";
import {
  categorySlices,
  financeStats,
  flowBars,
  signedMoney,
  transactionAmount,
} from "./display";

const months: TransactionMonth[] = [
  { year: 2026, month: 9, totalIncome: 4000, totalExpenses: 1500, net: 2500 },
  { year: 2026, month: 10, totalIncome: 6850, totalExpenses: 2140, net: 4710 },
];

const summary: TransactionSummary = {
  totalIncome: 6850,
  totalExpenses: 2140,
  net: 4710,
  mostActiveCategory: "Offerings",
  byCategory: [
    {
      categoryId: 1,
      categoryName: "Offerings",
      type: "Income",
      total: 6850,
      count: 5,
    },
    {
      categoryId: 2,
      categoryName: "Utilities",
      type: "Expense",
      total: 1240,
      count: 2,
    },
    {
      categoryId: 3,
      categoryName: "Printing",
      type: "Expense",
      total: 900,
      count: 1,
    },
  ],
};

describe("the signed amount", () => {
  it("marks money in with a plus and money out with a minus sign", () => {
    expect(transactionAmount({ amount: 6850, transactionType: "Income" })).toBe(
      "+GH₵ 6.85k",
    );
    expect(
      transactionAmount({ amount: 1240, transactionType: "Expense" }),
    ).toBe("−GH₵ 1.24k");
  });

  it("shows a net loss as a minus, never as a double sign", () => {
    expect(signedMoney(-900, "Expense")).toBe("−GH₵ 900");
  });
});

describe("the four numbers", () => {
  it("gives money in, money out, what is left and how many transactions", () => {
    expect(financeStats(summary).map((s) => s.value)).toEqual([
      "6.85k",
      "2.14k",
      "+4.71k",
      "8",
    ]);
  });

  it("waits with dashes until the numbers arrive", () => {
    expect(financeStats(undefined).map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
  });
});

describe("the categories", () => {
  it("shows the biggest of both types, largest first", () => {
    expect(categorySlices(summary, "").map((c) => c.label)).toEqual([
      "Offerings",
      "Utilities",
      "Printing",
    ]);
  });

  it("keeps to the type on show, and folds the smallest into Other", () => {
    expect(categorySlices(summary, "Expense").map((c) => c.label)).toEqual([
      "Utilities",
      "Printing",
    ]);
    const many = {
      ...summary,
      byCategory: [1, 2, 3, 4, 5, 6].map((n) => ({
        categoryId: n,
        categoryName: `C${n}`,
        type: "Expense" as const,
        total: 100 - n,
        count: 1,
      })),
    };
    const slices = categorySlices(many, "");
    expect(slices).toHaveLength(5);
    expect(slices[4]).toMatchObject({ label: "Other", value: 189 });
  });
});

describe("the chart", () => {
  it("gives every month both totals", () => {
    expect(flowBars(months)[1]).toEqual({
      key: "2026-10",
      label: "Oct",
      name: "October 2026",
      income: 6850,
      expenses: 2140,
    });
  });

  it("draws nothing when there are no months", () => {
    expect(flowBars([])).toEqual([]);
  });
});
