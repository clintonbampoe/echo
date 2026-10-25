// The numbers about a person's attendance for a profile. Pure, so the tests can check them without a screen.
// "–" means "still loading" or "nothing to show".
import { formatDayMonth } from "@/lib/format";
import type { Stat } from "@/ui";

// "86" for 38 of 44. Nothing when no sessions were held, so the card shows a dash instead of 0%.
export function attendancePercent(
  attended: number,
  held: number,
): number | null {
  if (held <= 0) return null;
  return Math.min(100, Math.round((attended / held) * 100));
}

export function attendanceStat(present?: number, held?: number): Stat {
  const known = present !== undefined && held !== undefined;
  const percent = known ? attendancePercent(present, held) : null;
  return {
    label: "Attendance",
    value: percent === null ? "–" : String(percent),
    unit: percent === null ? undefined : "%",
    note: known
      ? held > 0
        ? `${present} of ${held} sessions this year`
        : "No sessions held yet this year"
      : "",
  };
}

export function lastSeenStat(
  last: { sessionDate: string; name: string } | undefined,
  loading: boolean,
): Stat {
  if (!last)
    return {
      label: "Last seen",
      value: "–",
      note: loading ? "" : "No attendance yet",
    };
  return {
    label: "Last seen",
    value: formatDayMonth(last.sessionDate),
    note: last.name,
  };
}
