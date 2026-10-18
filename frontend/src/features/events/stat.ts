// The four numbers above the Events list. Pure, so the tests can check them without a screen.
import { formatDayTime, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { EventItem, EventSummary } from "./api";

// How many of the registered came, as the note under the "Checked in" number.
const cameNote = (s: EventSummary) =>
  s.totalRegistered > 0
    ? `${Math.round((s.totalAttended / s.totalRegistered) * 100)}% of those registered`
    : "No one registered yet";

// The note under "Upcoming": which event is next. `next` is null when none is planned, undefined while it loads.
const nextNote = (next: EventItem | null | undefined) =>
  next
    ? `Next: ${formatDayTime(next.startDate)}`
    : next === null
      ? "Nothing planned"
      : "Loading";

export function summaryStats(
  s: EventSummary | undefined,
  next: EventItem | null | undefined,
): Stat[] {
  const n = (value: number | undefined) =>
    value === undefined ? "–" : formatNumber(value);
  return [
    {
      label: "Events",
      value: n(s?.totalEvents),
      note: s ? `${formatNumber(s.upcomingEvents)} coming up` : "Loading",
    },
    { label: "Upcoming", value: n(s?.upcomingEvents), note: nextNote(next) },
    {
      label: "Registered",
      value: n(s?.totalRegistered),
      note: "Across all events",
    },
    {
      label: "Checked in",
      value: n(s?.totalAttended),
      note: s ? cameNote(s) : "Loading",
    },
  ];
}

// How many events each tab holds, for its badge. Both come from the summary, so they always agree with it.
export const tabCounts = (s?: EventSummary) => ({
  upcoming: s ? s.upcomingEvents : null,
  past: s ? Math.max(0, s.totalEvents - s.upcomingEvents) : null,
});
