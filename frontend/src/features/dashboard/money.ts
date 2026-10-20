// What the Money row draws: the months, and the slices of one kind of money. Pure.
import { monthNames, monthsAfter } from "@/lib/dates";
import type { CategoryTotal, MoneyMonth } from "./api";
import { shareOf } from "./shares";

// How many months the Money cards can show.
export const MONTH_RANGES = [3, 6, 12] as const;
export type MonthRange = (typeof MONTH_RANGES)[number];
export const DEFAULT_MONTHS: MonthRange = 6;

export const monthRangeLabel = (months: number) => `Last ${months} months`;

// The range switch's choices for the money months, as short as the card's heading allows: "3 months",
// "6 months", "Year". A tight row drops to "3 mo".
export const monthChoices = MONTH_RANGES.map((m) => ({
  value: String(m),
  label: m === 12 ? "Year" : `${m} months`,
  short: m === 12 ? "Year" : `${m} mo`,
}));

// The months ending with this one, as the days the API takes. This month counts as far as today.
export function moneyRange(today: string, months: number) {
  return {
    from: `${monthsAfter(today.slice(0, 7), -(months - 1))}-01`,
    to: today,
  };
}

const shortMonth = (month: number) => (monthNames[month - 1] ?? "").slice(0, 3);

// One month on the chart, known before any money has arrived.
export type MonthSlot = {
  key: string;
  label: string;
  name: string;
  tip: string;
};

export function monthSlots(today: string, months: number): MonthSlot[] {
  const thisMonth = today.slice(0, 7);
  return Array.from({ length: months }, (_, i) => {
    const key = monthsAfter(thisMonth, -(months - 1 - i));
    const [year = 0, m = 1] = key.split("-").map(Number);
    return {
      key,
      label: shortMonth(m),
      name: `${monthNames[m - 1] ?? ""} ${year}`,
      tip: `${shortMonth(m)} ${year}`,
    };
  });
}

// A month with its money. Without `income` and `expenses` the money has not arrived yet.
export type Figures = { slot: MonthSlot; income?: number; expenses?: number };

export function monthFigures(
  slots: readonly MonthSlot[],
  months: readonly MoneyMonth[] | undefined,
): Figures[] {
  return slots.map((slot) => {
    if (!months) return { slot };
    const found = months.find(
      (m) => `${m.year}-${String(m.month).padStart(2, "0")}` === slot.key,
    );
    return {
      slot,
      income: found?.totalIncome ?? 0,
      expenses: found?.totalExpenses ?? 0,
    };
  });
}

export type MoneyKind = "Income" | "Expense";

// The slices of one kind of money: the biggest four categories, and the rest under "Other".
export function slicesOf(
  categories: readonly CategoryTotal[],
  kind: MoneyKind,
) {
  return shareOf(
    categories
      .filter((c) => c.type === kind)
      .map((c) => ({
        key: String(c.categoryId),
        name: c.categoryName,
        total: c.total,
      })),
  );
}
