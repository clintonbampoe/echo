import {
  dayParts,
  daysAfter,
  daysBetween,
  monthNames,
  monthStart,
  monthsBefore,
  weekStart,
} from "@/lib/dates";
import { formatDayMonth } from "@/lib/format";

export const periodKeys = [
  "this-week",
  "last-week",
  "this-month",
  "last-3-months",
  "last-6-months",
  "this-year",
] as const;
export type PeriodKey = (typeof periodKeys)[number];

export const DEFAULT_PERIOD: PeriodKey = "this-week";

export const isPeriodKey = (value: unknown): value is PeriodKey =>
  periodKeys.some((key) => key === value);

// A stretch of days, both ends included, as plain "2026-10-19" days.
export type Span = { from: string; to: string };

// `label` names it on the pill. `before` is what sits under a number compared with the stretch before.
export type Period = { key: PeriodKey; label: string; before: string };

const words: Record<PeriodKey, { label: string; before: string }> = {
  "this-week": { label: "This week", before: "vs the days before" },
  "last-week": { label: "Last week", before: "vs the week before" },
  "this-month": { label: "This month", before: "vs the stretch before" },
  "last-3-months": {
    label: "Last 3 months",
    before: "vs the 3 months before",
  },
  "last-6-months": {
    label: "Last 6 months",
    before: "vs the 6 months before",
  },
  "this-year": { label: "This year", before: "vs the stretch before" },
};

export const periodOf = (key: PeriodKey): Period => ({ key, ...words[key] });

export const periods: readonly Period[] = periodKeys.map(periodOf);

// Weeks run Sunday to Saturday, as on the server. Pure, so the tests can pass "today".
const spans: Record<PeriodKey, (today: string) => Span> = {
  "this-week": (today) => ({ from: weekStart(today), to: today }),
  "last-week": (today) => {
    const start = weekStart(today);
    return { from: daysAfter(start, -7), to: daysAfter(start, -1) };
  },
  "this-month": (today) => ({ from: monthStart(today), to: today }),
  "last-3-months": (today) => ({
    from: daysAfter(monthsBefore(today, 3), 1),
    to: today,
  }),
  "last-6-months": (today) => ({
    from: daysAfter(monthsBefore(today, 6), 1),
    to: today,
  }),
  "this-year": (today) => ({ from: `${today.slice(0, 4)}-01-01`, to: today }),
};

export const spanOf = (key: PeriodKey, today: string): Span =>
  spans[key](today);

// The stretch of the same length that ends the day before this one starts.
export function previousSpan({ from, to }: Span): Span {
  const length = daysBetween(from, to) + 1;
  return { from: daysAfter(from, -length), to: daysAfter(from, -1) };
}

// For the menu: "11 – 17 Oct", "20 Jul – 19 Oct", or "18 Oct" for a single day.
export function rangeHint({ from, to }: Span): string {
  if (from === to) return formatDayMonth(to);
  const sameMonth = from.slice(0, 7) === to.slice(0, 7);
  const start = sameMonth ? dayParts(from)[2] : formatDayMonth(from);
  return `${start} – ${formatDayMonth(to)}`;
}

// For a sentence: "11 to 17 October", "20 July to 19 October".
export function spokenSpan({ from, to }: Span): string {
  const [, fromMonth, fromDay] = dayParts(from);
  const [, toMonth, toDay] = dayParts(to);
  const end = `${toDay} ${monthNames[toMonth - 1]}`;
  if (from === to) return end;
  const start =
    fromMonth === toMonth
      ? `${fromDay}`
      : `${fromDay} ${monthNames[fromMonth - 1]}`;
  return `${start} to ${end}`;
}
