// How a row of the Events list says things. Pure, so the tests can check it without a screen.
import { monthNames, dayParts } from "@/lib/dates";
import { formatDayTime, formatNumber } from "@/lib/format";
import type { EventItem } from "./api";

// "Sat 17 Oct, 4:00 pm". An event over several days gives the first and last day: "Fri 30 Oct to Sun 1 Nov".
export function eventWhen(
  e: Pick<EventItem, "startDate" | "endDate" | "startTime">,
): string {
  const first = e.startDate.slice(0, 10);
  const last = e.endDate.slice(0, 10);
  return last > first
    ? `${formatDayTime(first)} to ${formatDayTime(last)}`
    : formatDayTime(first, e.startTime);
}

// "Sat 17 Oct, 4:00 pm · Main Auditorium". Without a place, just the time.
export const eventWhere = (
  e: Pick<EventItem, "startDate" | "endDate" | "startTime" | "location">,
): string => (e.location ? `${eventWhen(e)} · ${e.location}` : eventWhen(e));

// The month and day for the square at the start of a row: "Oct" and "17".
export function dateTile(day: string): { month: string; date: string } {
  const [, month, date] = dayParts(day);
  return {
    month: (monthNames[month - 1] ?? "").slice(0, 3),
    date: String(date),
  };
}

// How full an event is, from 0 to 1. An event with no limit has no share.
export const capacityShare = (
  registered: number,
  capacity?: number | null,
): number | null => (capacity ? Math.min(1, registered / capacity) : null);

export const registeredLabel = (n: number) => `${formatNumber(n)} registered`;
export const capacityLabel = (capacity?: number | null) =>
  capacity ? `of ${formatNumber(capacity)}` : "No limit";
