import { thisYear } from "@/lib/dates";
import { failedStat, type Stat } from "@/ui";
import { useAttendance, useAttendanceCount, useHeldCount } from "./queries";
import { attendanceStat, lastSeenStat } from "../model/figures";

// When a person last came, as a card. Visitors and members both have one; `label` is what the page calls it.
export function useLastSeen(personId: string, label = "Last seen"): Stat {
  const list = useAttendance(personId);
  const newest = list.rows[0];
  const stat = lastSeenStat(
    newest && {
      sessionDate: newest.sessionDate,
      name: newest.sessionName || newest.sessionLabelName,
    },
    list.loading,
  );
  const named = { ...stat, label };
  return list.failed ? failedStat(named) : named;
}

// The two numbers a profile shows about attendance: how much of this year they came to, and when they last did.
export function usePersonStats(personId: string, today: string): Stat[] {
  const year = thisYear(today);
  const attended = useAttendanceCount(personId, year.from, year.to);
  const held = useHeldCount(year.from, year.to);
  const list = useAttendance(personId);
  const newest = list.rows[0];

  const percent = attendanceStat(
    attended.data?.totalPresent,
    held.data?.completedSessions,
  );
  const last = lastSeenStat(
    newest && {
      sessionDate: newest.sessionDate,
      name: newest.sessionName || newest.sessionLabelName,
    },
    list.loading,
  );
  return [
    attended.isError || held.isError ? failedStat(percent) : percent,
    list.failed ? failedStat(last) : last,
  ];
}
