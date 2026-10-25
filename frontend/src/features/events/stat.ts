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

const count = (value: number | undefined) =>
  value === undefined ? "–" : formatNumber(value);

// What the strip says over the upcoming events: how many, how many have signed up, and how many events there are.
function upcomingStats(
  s: EventSummary | undefined,
  next: EventItem | null | undefined,
): Stat[] {
  return [
    {
      label: "Upcoming",
      value: count(s?.upcomingEvents),
      note: nextNote(next),
    },
    {
      label: "Registered",
      value: count(s?.totalRegistered),
      note: "Across all events",
    },
    {
      label: "Events",
      value: count(s?.totalEvents),
      note: s ? `${formatNumber(s.upcomingEvents)} coming up` : "Loading",
    },
  ];
}

// Over the past ones: how many were held, how many checked in, and how many came to an event on average.
function pastStats(s: EventSummary | undefined): Stat[] {
  const held = s ? Math.max(0, s.totalEvents - s.upcomingEvents) : undefined;
  const average =
    s && held ? Math.round(s.totalAttended / held) : s ? 0 : undefined;
  return [
    { label: "Events held", value: count(held), note: "Before today" },
    {
      label: "Check-ins",
      value: count(s?.totalAttended),
      note: s ? cameNote(s) : "Loading",
    },
    { label: "Average", value: count(average), note: "Check-ins per event" },
  ];
}

export const summaryStats = (
  s: EventSummary | undefined,
  next: EventItem | null | undefined,
  tab: "upcoming" | "past",
): Stat[] => (tab === "past" ? pastStats(s) : upcomingStats(s, next));
