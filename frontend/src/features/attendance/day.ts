// What the Attendance page shows for one day, worked out from plain days and sessions. Pure, so the tests
// can pass "today". No React in this file.
import { dayParts, daysAfter, daysBetween, monthNames } from "@/lib/dates";
import { formatClock, formatNumber } from "@/lib/format";
import type { ChipTone, Stat, StripDay } from "@/ui";
import type { AttendanceSummary, Session, SessionStatus } from "./api";

// The strip lists this many days either side of its anchor, and the anchor only moves once the picked day gets
// further than REANCHOR from it, so scrolling never rebuilds the row under your finger.
const STRIP_RADIUS = 45;
const REANCHOR = 30;
// The server is asked for the sessions around the picked day, for the dots.
const DOTS_RADIUS = 21;

export const needsNewAnchor = (anchor: string, day: string) =>
  daysBetween(anchor, day) + daysBetween(day, anchor) > REANCHOR;

export function stripRange(selected: string): { from: string; to: string } {
  return {
    from: daysAfter(selected, -DOTS_RADIUS),
    to: daysAfter(selected, DOTS_RADIUS),
  };
}

const weekday = (day: string, style: "long" | "short") => {
  const [y, m, d] = dayParts(day);
  return new Date(Date.UTC(y, m - 1, d)).toLocaleDateString("en-GB", {
    weekday: style,
    timeZone: "UTC",
  });
};

// "2026-10-11" -> "Sunday, 11 October 2026".
export function longDay(day: string): string {
  const [y, m, d] = dayParts(day);
  return `${weekday(day, "long")}, ${d} ${monthNames[m - 1]} ${y}`;
}

// "2026-10-11" -> "Sunday, 11 October". For a way back, where the year is understood.
export const dayName = (day: string) => longDay(day).replace(/ \d{4}$/, "");

// "2026-10-14" -> "Wed 14 Oct".
export function shortDay(day: string): string {
  const [, m, d] = dayParts(day);
  return `${weekday(day, "short")} ${d} ${(monthNames[m - 1] ?? "").slice(0, 3)}`;
}

// The days of the strip, around its anchor, each with how many sessions fall on it.
export function stripDays(
  anchor: string,
  today: string,
  sessions: Pick<Session, "date">[],
): StripDay[] {
  const from = daysAfter(anchor, -STRIP_RADIUS);
  return Array.from({ length: STRIP_RADIUS * 2 + 1 }, (_, i) => {
    const day = daysAfter(from, i);
    const [, month, date] = dayParts(day);
    const tag =
      day === today
        ? "Today"
        : date === 1
          ? (monthNames[month - 1] ?? "").slice(0, 3)
          : undefined;
    return {
      day,
      weekday: weekday(day, "short"),
      date,
      marks: sessions.filter((s) => s.date === day).length,
      today: day === today,
      ahead: day > today,
      tag,
    };
  });
}

export const sessionTitle = (s: Pick<Session, "name" | "sessionLabelName">) =>
  s.name || s.sessionLabelName;

export const statusTone: Record<SessionStatus, ChipTone> = {
  Scheduled: "neutral",
  Completed: "positive",
  Cancelled: "warning",
};

// "8:00 am to 10:30 am", "8:00 am", or nothing when no time was set.
export function timeRange(s: Pick<Session, "startsAt" | "endsAt">): string {
  if (!s.startsAt) return "";
  return s.endsAt
    ? `${formatClock(s.startsAt)} to ${formatClock(s.endsAt)}`
    : formatClock(s.startsAt);
}

const plural = (n: number, one: string, many: string) =>
  `${formatNumber(n)} ${n === 1 ? one : many}`;
export const sessionCount = (n: number) => plural(n, "session", "sessions");

// The four numbers of a day or a session. `present` is the line under the total ("Across 2 sessions") and
// `idle` the line under members and visitors while nobody is marked yet.
export function presenceStats(
  summary: AttendanceSummary | undefined,
  present: string,
  idle: string,
  presentLabel = "Present",
): Stat[] {
  if (!summary) {
    return [presentLabel, "Members", "Visitors", "First-time visitors"].map(
      (label) => ({ label, value: "–", note: "" }),
    );
  }
  const {
    totalPresent: total,
    membersPresent: members,
    visitorsPresent: visitors,
    firstTimeVisitors,
  } = summary;
  const members100 = total > 0 ? Math.round((members / total) * 100) : 0;
  const share = (n: number, percent: number) => ({
    value: formatNumber(n),
    note: total > 0 ? `${percent}% of those present` : idle,
  });
  return [
    {
      label: presentLabel,
      value: formatNumber(total),
      note: total > 0 ? present : idle,
    },
    { label: "Members", ...share(members, members100) },
    { label: "Visitors", ...share(visitors, 100 - members100) },
    {
      label: "First-time visitors",
      value: formatNumber(firstTimeVisitors),
      note: "Came for the first time",
    },
  ];
}

// The day in the address ("?day=2026-10-11"), or today when it is missing or not a real day.
export function readDay(text: string | null, today: string): string {
  if (!text || !/^\d{4}-\d{2}-\d{2}$/.test(text)) return today;
  const [y, m, d] = dayParts(text);
  return new Date(Date.UTC(y, m - 1, d)).toISOString().slice(0, 10) === text
    ? text
    : today;
}

// "2026-10-11" -> "11 Oct".
export function dayMonth(day: string): string {
  const [, m, d] = dayParts(day);
  return `${d} ${(monthNames[m - 1] ?? "").slice(0, 3)}`;
}

// The nearest other day that has a session: the next one after `day`, else the latest one before it.
export function nearestSessionDay(
  day: string,
  sessions: Pick<Session, "date">[],
): string | null {
  const dates = sessions.map((s) => s.date);
  const after = dates.filter((d) => d > day).sort()[0];
  return (
    after ??
    dates
      .filter((d) => d < day)
      .sort()
      .pop() ??
    null
  );
}
