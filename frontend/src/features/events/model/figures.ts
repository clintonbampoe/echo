// The numbers above the Events list. Each is a figure and a label, nothing more.
import { formatDayMonth, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { EventItem, EventSummary } from "../api/events";

const count = (value: number | undefined) =>
  value === undefined ? "–" : formatNumber(value);

// The day of the next event. A dash while it loads, "None" when nothing is planned.
const nextDay = (next: EventItem | null | undefined) =>
  next ? formatDayMonth(next.startDate) : next === null ? "None" : "–";

// Over the upcoming events: how many, how many have signed up, and when the next one is.
function upcomingStats(
  s: EventSummary | undefined,
  next: EventItem | null | undefined,
): Stat[] {
  return [
    { label: "Upcoming", value: count(s?.upcomingEvents) },
    { label: "Registered", value: count(s?.totalRegistered) },
    { label: "Next event", value: nextDay(next) },
  ];
}

// Over the past ones: how many were held, how many checked in, and how many came to an event on average.
function pastStats(s: EventSummary | undefined): Stat[] {
  const held = s ? Math.max(0, s.totalEvents - s.upcomingEvents) : undefined;
  const average =
    s && held ? Math.round(s.totalAttended / held) : s ? 0 : undefined;
  return [
    { label: "Events held", value: count(held) },
    { label: "Check-ins", value: count(s?.totalAttended) },
    { label: "Average", value: count(average) },
  ];
}

export const summaryStats = (
  s: EventSummary | undefined,
  next: EventItem | null | undefined,
  tab: "upcoming" | "past",
): Stat[] => (tab === "past" ? pastStats(s) : upcomingStats(s, next));
