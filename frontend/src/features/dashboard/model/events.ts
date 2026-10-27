// How the Events row says things, and which days of a month have an event. Pure.
import { daysAfter, monthNames } from "@/lib/dates";
import type { DashEvent } from "../api/events";

// "October 2026" for "2026-10".
export function monthTitle(month: string): string {
  const [year = "", m = "1"] = month.split("-");
  return `${monthNames[Number(m) - 1] ?? ""} ${year}`;
}

// An event longer than this many days is drawn only for its first ones.
const MAX_DAYS = 31;

// The days of one month that have an event, with the names of the events on each. An event over several days
// marks each of them.
export function eventDays(
  events: readonly DashEvent[],
  month: string,
): Map<string, string[]> {
  const days = new Map<string, string[]>();
  for (const e of events) {
    let day = e.startDate;
    for (let i = 0; day <= e.endDate && i < MAX_DAYS; i++) {
      if (day.startsWith(month))
        days.set(day, [...(days.get(day) ?? []), e.name]);
      day = daysAfter(day, 1);
    }
  }
  return days;
}

// A day with an event has not happened yet (today counts as not yet), or has been held.
export const kindOf = (day: string, today: string): "held" | "upcoming" =>
  day < today ? "held" : "upcoming";

// The day shown first: the next one with an event, or when they are all over, the latest.
export function defaultPick(
  days: ReadonlyMap<string, string[]>,
  today: string,
): string | undefined {
  const sorted = [...days.keys()].sort();
  return sorted.find((d) => d >= today) ?? sorted[sorted.length - 1];
}
