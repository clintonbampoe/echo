// What the Finance page is narrowed by. The mechanics (reading and writing the address) are shared in lib;
// this file says which filters finance has and how they become API filters. No React in this file.
import { ALL_TIME, monthNames, monthStart, weekStart } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { type Option } from "@/ui";
import { withAny } from "@/lib/options";
import type { Category } from "../api/categories";
import type { TransactionFilters, TransactionType } from "../api/transactions";

export const transactionTypes: readonly TransactionType[] = [
  "Income",
  "Expense",
];

// The days the page covers. The default is this month, as the boards show.
export type PeriodKey = "week" | "month" | "year" | "last-year" | "all";

export const periodKeys: readonly PeriodKey[] = [
  "week",
  "month",
  "year",
  "last-year",
  "all",
];

export const periodOptions: readonly { value: PeriodKey; label: string }[] = [
  { value: "month", label: "This month" },
  { value: "week", label: "This week" },
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
  { value: "all", label: "All time" },
];

// Money in, money out, or both.
export const typeOptions: readonly Option[] = withAny("Type", [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
]);

export const financeFilters = urlFilters(
  { type: "", category: "", month: "", window: "month" },
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

// '2026-10' -> that month's days; '2026' -> the whole year; anything else -> nothing, so the period decides.
export function monthDates(
  value: string,
): { from: string; to: string } | undefined {
  const m = /^(\d{4})(?:-(\d{2}))?$/.exec(value);
  if (!m) return undefined;
  const year = Number(m[1]);
  if (!m[2]) return { from: `${year}-01-01`, to: `${year}-12-31` };
  const month = Number(m[2]);
  const last = new Date(Date.UTC(year, month, 0)).getUTCDate();
  const mm = String(month).padStart(2, "0");
  return { from: `${year}-${mm}-01`, to: `${year}-${mm}-${last}` };
}

// '2026-10' -> "October 2026"; '2026' -> "2026".
export function monthLabel(value: string): string {
  const [year = "", month] = value.split("-");
  return month ? `${monthNames[Number(month) - 1] ?? ""} ${year}` : year;
}

// Filters narrowing the list, not counting the type switch (which is always in view).
export const countActive = (f: ListFilters) =>
  (f.category ? 1 : 0) + (f.month ? 1 : 0) + (f.window === "month" ? 0 : 1);

export function toApiFilters(
  f: ListFilters,
  today: string,
): TransactionFilters {
  return {
    transactionType: typeOf(f.type),
    categoryId: categoryOf(f.category),
    ...(monthDates(f.month) ?? periodDates(f.window, today)),
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

// The last twelve (or six) months, whatever days the list is showing: the chart is the long view.
export const chartRange = (
  today: string,
  months = 12,
): { from: string; to: string } => {
  const [year, month] = [Number(today.slice(0, 4)), Number(today.slice(5, 7))];
  const from = new Date(Date.UTC(year, month - months, 1))
    .toISOString()
    .slice(0, 10);
  return { from, to: today };
};
