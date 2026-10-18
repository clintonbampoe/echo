// The line under an event's name. Pure, so the tests can check it without a screen.
import { dayParts, monthNames } from "@/lib/dates";
import { formatClock } from "@/lib/format";
import type { EventItem } from "./api";

// "Sat 3 Oct 2026". With a time: "Sat 3 Oct 2026, 6:00 am".
function longDay(day: string, time?: string | null): string {
  const [year, month, date] = dayParts(day);
  const weekday = new Date(Date.UTC(year, month - 1, date)).toLocaleDateString(
    "en-GB",
    { weekday: "short", timeZone: "UTC" },
  );
  const text = `${weekday} ${date} ${(monthNames[month - 1] ?? "").slice(0, 3)} ${year}`;
  return time ? `${text}, ${formatClock(time)}` : text;
}

// When it is: "Sat 3 Oct 2026, 6:00 am to 6:00 pm" for one day, or both ends for several days.
export function eventDates(
  e: Pick<EventItem, "startDate" | "endDate" | "startTime" | "endTime">,
): string {
  const first = e.startDate.slice(0, 10);
  const last = e.endDate.slice(0, 10);
  if (last > first)
    return `${longDay(first, e.startTime)} to ${longDay(last, e.endTime)}`;
  return e.endTime
    ? `${longDay(first, e.startTime)} to ${formatClock(e.endTime)}`
    : longDay(first, e.startTime);
}

// "Sat 3 Oct 2026, 6:00 am to 6:00 pm. Aburi Mountain Lodge. Organized by Efua Quaye."
export function eventHeadline(
  e: Pick<
    EventItem,
    | "startDate"
    | "endDate"
    | "startTime"
    | "endTime"
    | "location"
    | "organizerName"
  >,
): string {
  return (
    [eventDates(e), e.location, `Organized by ${e.organizerName}`]
      .filter(Boolean)
      .join(". ") + "."
  );
}
