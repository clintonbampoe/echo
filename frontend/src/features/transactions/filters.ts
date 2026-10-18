// What the Finance page is narrowed by. The mechanics (reading and writing the address) are shared in lib;
// this file says which filters finance has and how they become API filters. No React in this file.
import { ALL_TIME, monthStart, weekStart } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { withAny, type Option } from "@/ui";
import type { Category, TransactionFilters, TransactionType } from "./api";

export const transactionTypes: readonly TransactionType[] = [
  "Income",
  "Expense",
];

// The days the page covers. The default is this week, as the server's is.
export type PeriodKey = "week" | "month" | "year" | "last-year" | "all";

export const periodKeys: readonly PeriodKey[] = [
  "week",
  "month",
  "year",
  "last-year",
  "all",
];

export const periodOptions: readonly { value: PeriodKey; label: string }[] = [
  { value: "week", label: "This week" },
  { value: "month", label: "This month" },
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
  { value: "all", label: "All time" },
];

// Money in, money out, or both. A mode switch, not a pill: it stays in view on every screen (design rule S2).
export const typeOptions = [
  { value: "", label: "All" },
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
] as const;

export const financeFilters = urlFilters(
  { type: "", category: "", window: "week" },
  { type: ["", ...transactionTypes], window: periodKeys },
);

export type ListFilters = typeof financeFilters.empty;

// The type a filter value names, or nothing for 'all' (or text that is no type).
export const typeOf = (value: string): TransactionType | undefined =>
  transactionTypes.find((t) => t === value);

// The category a filter value names. Ids are numbers in the API and text in the address.
export const categoryOf = (value: string): number | undefined =>
  /^\d+$/.test(value) ? Number(value) : undefined;

// Only the categories of the type on show: an income category can hold no expense, so offering it would be a lie.
export const categoryOptions = (
  categories: Category[] | undefined,
  type: string,
): readonly Option[] =>
  withAny(
    "Category",
    (categories ?? [])
      .filter((c) => !type || c.categoryType === type)
      .map((c) => ({ value: String(c.id), label: c.name })),
  );

export function periodDates(
  key: string,
  today: string,
): { from: string; to: string } {
  const year = Number(today.slice(0, 4));
  if (key === "month") return { from: monthStart(today), to: today };
  if (key === "year") return { from: `${year}-01-01`, to: today };
  if (key === "last-year")
    return { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` };
  if (key === "all") return ALL_TIME;
  return { from: weekStart(today), to: today };
}

// Filters narrowing the list, not counting the type switch (which is always in view).
export const countActive = (f: ListFilters) =>
  (f.category ? 1 : 0) + (f.window === "week" ? 0 : 1);

export function toApiFilters(
  f: ListFilters,
  today: string,
): TransactionFilters {
  return {
    transactionType: typeOf(f.type),
    categoryId: categoryOf(f.category),
    ...periodDates(f.window, today),
  };
}

// Switching the type can leave a category that no longer belongs to it, so that choice is dropped.
export function chooseType(
  f: ListFilters,
  type: string,
  categories: Category[] | undefined,
): Partial<ListFilters> {
  const picked = (categories ?? []).find((c) => String(c.id) === f.category);
  const keep = !type || !picked || picked.categoryType === type;
  return { type, category: keep ? f.category : "" };
}

// The last twelve months, whatever days the list is showing: the chart is the long view.
export const chartRange = (today: string): { from: string; to: string } => {
  const [year, month] = [Number(today.slice(0, 4)), Number(today.slice(5, 7))];
  const from = new Date(Date.UTC(year, month - 12, 1))
    .toISOString()
    .slice(0, 10);
  return { from, to: today };
};
