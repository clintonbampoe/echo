import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { todayText, daysAfter } from "@/lib/dates";
import { DayStrip, StatStrip } from "@/ui";
import {
  presenceStats,
  readDay,
  sessionCount,
  stripDays,
  stripRange,
} from "../day";
import {
  useAttendanceSummary,
  useSessionLabels,
  useSessionsBetween,
} from "../hooks";
import { useStripAnchor } from "../useStripAnchor";
import { AttendanceHeader } from "./AttendanceHeader";
import { DayBar } from "./DayBar";
import { DaySessions } from "./DaySessions";
import { SessionSheet } from "./SessionSheet";
import styles from "./AttendancePage.module.css";

// The Attendance page: pick a day on the strip, see its sessions and who came, open one to mark people present.
export function AttendancePage() {
  const today = todayText();
  const [params, setParams] = useSearchParams();
  const day = readDay(params.get("day"), today);
  const [creating, setCreating] = useState(false);
  const anchor = useStripAnchor(day);
  const { from, to } = stripRange(day);
  const sessions = useSessionsBetween(from, to);
  const labels = useSessionLabels();
  const summary = useAttendanceSummary({ from: day, to: day });
  const pick = (next: string) =>
    setParams(next === today ? {} : { day: next }, { replace: true });
  const here = (sessions.data ?? []).filter((s) => s.date === day);
  // The strip may still be showing the last window while the next loads, so a day is only "empty" once its own answer is in.
  const ready = Boolean(sessions.data) && !sessions.isPlaceholderData;

  return (
    <div className="anim-rise">
      <AttendanceHeader tab="day" onNew={() => setCreating(true)} />
      <div
        role="tabpanel"
        id="attendance-panel-day"
        aria-labelledby="attendance-tab-day"
        className={styles.panel}
      >
        <div>
          <DayStrip
            label="Days"
            days={stripDays(anchor, today, sessions.data ?? [])}
            selected={day}
            onSelect={pick}
            onShift={(weeks) => pick(daysAfter(day, weeks * 7))}
            noun={["session", "sessions"]}
          />
        </div>
        <DayBar day={day} today={today} onPick={pick} />
        <div>
          <StatStrip
            label="Attendance for this day"
            stats={presenceStats(
              summary.data,
              `Across ${sessionCount(here.length)}`,
              "No one marked",
              "Present",
            )}
            busy={summary.isPending}
          />
        </div>
        <div key={day} className="anim-rise">
          <DaySessions
            day={day}
            here={here}
            around={sessions.data ?? []}
            ready={ready}
            failed={sessions.isError}
            noTypes={labels.data?.length === 0}
            onPick={pick}
            onNew={() => setCreating(true)}
          />
        </div>
      </div>
      <SessionSheet
        open={creating}
        day={day}
        onClose={() => setCreating(false)}
        onCreated={(s) => pick(s.date)}
      />
    </div>
  );
}
