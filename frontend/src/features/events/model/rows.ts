// How a row of the Events list says things. Pure, so the tests can check it without a screen.
import { formatDayTime, formatNumber } from "@/lib/format";
import type { EventItem } from "../api/events";

// "Sat 17 Oct · Main Auditorium": the day, and the place when there is one. An event over several days gives the
// first and last day: "Fri 30 Oct to Sun 1 Nov".
export function eventSub(
  e: Pick<EventItem, "startDate" | "endDate" | "location">,
): string {
  const first = e.startDate.slice(0, 10);
  const last = e.endDate.slice(0, 10);
  const days =
    last > first
      ? `${formatDayTime(first)} to ${formatDayTime(last)}`
      : formatDayTime(first);
  return e.location ? `${days} · ${e.location}` : days;
}

// How full an event is, from 0 to 1. An event with no limit has no share.
export const capacityShare = (
  registered: number,
  capacity?: number | null,
): number | null => (capacity ? Math.min(1, registered / capacity) : null);

// What the right of a row says: an upcoming event, how much of its room is taken ("Registered 36% of 60"); a past
// one, how many of those registered came ("Came 93% of 210"), when the list knows. With nothing to measure
// against, just the count.
export type Fill = { label: string; text: string; share: number | null };

export function fillOf(e: EventItem, past: boolean): Fill {
  const came = past && e.attendanceCount !== undefined;
  const count = came ? (e.attendanceCount ?? 0) : e.registrationCount;
  const of = came ? e.registrationCount : e.capacity;
  const label = came ? "Came" : "Registered";
  const share = capacityShare(count, of);
  return {
    label,
    share,
    text:
      share === null || !of
        ? `${formatNumber(count)}${of === undefined || of === null ? " · No limit" : ""}`
        : `${Math.round(share * 100)}% of ${formatNumber(of)}`,
  };
}

// The line under a name in the list: how many signed up, or for a past event how many came when the list knows.
export function countOf(e: EventItem, past: boolean): string {
  const came = past && e.attendanceCount !== undefined;
  return `${formatNumber(came ? (e.attendanceCount ?? 0) : e.registrationCount)} ${came ? "came" : "registered"}`;
}
