// What the Events list is narrowed by, and what each tab asks the server for. The mechanics (reading and
// writing the address, the time range) are shared in lib. No React in this file.
import { ALL_TIME, daysAfter } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { windowKeys } from "@/lib/windows";
import type { EventQuery } from "../api/events";

// Upcoming is today and later, soonest first. Past is before today, newest first. Each tab is its own address.
export type EventTab = "upcoming" | "past";

// The time range is by the day an event starts, and it opens on this year, as the design does.
export const eventFilters = urlFilters(
  { name: "", organization: "", window: "year" },
  { window: windowKeys },
);

export type ListFilters = typeof eventFilters.empty;

// Filters that are narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: ListFilters) =>
  (f.organization ? 1 : 0) + (f.window === "year" ? 0 : 1);

// The first and last day of a time range. Unlike other lists, "this year" is spelled out: an upcoming
// tab must not start before today, and that needs a real day to compare with.
export function windowRange(
  key: string,
  now: Date,
): { from: string; to: string } {
  const year = now.getFullYear();
  if (key === "last-year")
    return { from: `${year - 1}-01-01`, to: `${year - 1}-12-31` };
  if (key === "all") return ALL_TIME;
  return { from: `${year}-01-01`, to: `${year}-12-31` };
}

// What a tab asks for: its time range cut at today. A range left with nothing in it (upcoming events of
// last year) comes out with `from` after `to`, which the list reads as "ask no one".
export function toApiFilters(
  f: ListFilters,
  tab: EventTab,
  today: string,
  now = new Date(),
): EventQuery {
  const range = windowRange(f.window, now);
  const base = {
    name: f.name.trim() || undefined,
    organizationId: f.organization || undefined,
  };
  if (tab === "upcoming")
    return {
      ...base,
      from: range.from > today ? range.from : today,
      to: range.to,
      order: "Asc",
    };
  const yesterday = daysAfter(today, -1);
  return {
    ...base,
    from: range.from,
    to: range.to < yesterday ? range.to : yesterday,
    order: "Desc",
  };
}
