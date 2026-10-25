// What the Tithes page is narrowed by. The mechanics (reading and writing the address) are shared in lib;
// this file says which filters tithes have and how they become API filters. No React in this file.
import { paymentLabel, type PaymentMethod } from "@/features/transactions";
import { ALL_TIME, monthNames, monthStart, weekStart } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { withAny, type Option } from "@/ui";
import type { TitheFilters } from "../api/tithes";

// When a tithe was collected. The default is this week, as the server's is.
export type CollectedKey = "week" | "month" | "year" | "last-year" | "all";

export const collectedKeys: readonly CollectedKey[] = [
  "week",
  "month",
  "year",
  "last-year",
  "all",
];

export const collectedOptions: readonly {
  value: CollectedKey;
  label: string;
}[] = [
  { value: "week", label: "Collected this week" },
  { value: "month", label: "Collected this month" },
  { value: "year", label: "Collected this year" },
  { value: "last-year", label: "Collected last year" },
  { value: "all", label: "Collected any time" },
];

const methods = Object.keys(paymentLabel) as PaymentMethod[];

// The method a filter value names, or nothing for 'any' (or text that is no method).
export const methodOf = (value: string): PaymentMethod | undefined =>
  methods.find((m) => m === value);

// `forMonth` is the period a tithe is for: '2026-10' for one month, '2026' for a whole year, '' for any.
export const titheFilters = urlFilters(
  { name: "", forMonth: "", method: "", window: "week" },
  { method: methods, window: collectedKeys },
);

export type ListFilters = typeof titheFilters.empty;

export const methodOptions: readonly Option[] = withAny("Method", [
  ...methods.map((value) => ({ value, label: paymentLabel[value] })),
]);

// '2026-10' -> { year: 2026, month: 10 }; '2026' -> { year: 2026 }; anything else -> nothing.
export function parseForMonth(
  value: string,
): { year: number; month?: number } | null {
  const match = /^(\d{4})(?:-(0[1-9]|1[0-2]))?$/.exec(value);
  if (!match) return null;
  return {
    year: Number(match[1]),
    month: match[2] ? Number(match[2]) : undefined,
  };
}

// "October 2026" or "2026", for the pill.
export function forMonthLabel(value: string): string {
  const parsed = parseForMonth(value);
  if (!parsed) return "";
  return parsed.month
    ? `${monthNames[parsed.month - 1]} ${parsed.year}`
    : String(parsed.year);
}

export function collectedDates(
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

// Filters that are narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: ListFilters) =>
  (f.forMonth ? 1 : 0) + (f.method ? 1 : 0) + (f.window === "week" ? 0 : 1);

// Every window sends its days, so "Tithe for October" and "Collected this week" always mean both.
export function toApiFilters(f: ListFilters, today: string): TitheFilters {
  const period = parseForMonth(f.forMonth);
  return {
    memberName: f.name.trim() || undefined,
    year: period?.year,
    month: period?.month ? monthNames[period.month - 1] : undefined,
    paymentMethod: methodOf(f.method),
    ...collectedDates(f.window, today),
  };
}

// The days a member's tithes are limited to. Unlike the Tithes page, "all" sends no dates at all: a request for
// one member has no default range, so nothing is cut off.
export type MemberWindow = "all" | "year" | "last-year";

export const memberWindowOptions: readonly {
  value: MemberWindow;
  label: string;
}[] = [
  { value: "all", label: "All time" },
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
];

export function memberTitheFilters(
  method: string,
  window: string,
  today: string,
): TitheFilters {
  const year = Number(today.slice(0, 4));
  const days =
    window === "year"
      ? { from: `${year}-01-01`, to: `${year}-12-31` }
      : window === "last-year"
        ? { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` }
        : {};
  return { paymentMethod: methodOf(method), ...days };
}

// Choosing the period a tithe is for is asking about that period, not about this week: while the date range
// is still the default, widen it so the answer is not empty by surprise.
export function chooseForMonth(
  f: ListFilters,
  forMonth: string,
): Partial<ListFilters> {
  return {
    forMonth,
    window: forMonth && f.window === "week" ? "all" : f.window,
  };
}
