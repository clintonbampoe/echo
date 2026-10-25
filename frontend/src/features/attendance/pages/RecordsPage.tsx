import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { AttendanceCard } from "@/features/dashboard";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { SegmentedField, StatStrip } from "@/ui";
import { presenceStats } from "../model/day";
import { useAttendanceSummary, useSessionLabels } from "../hooks/queries";
import {
  isNarrowed,
  recordFilters,
  toCheckInQuery,
  toSessionFilters,
  windowDates,
} from "../model/records";
import { AttendanceHeader } from "../components/day/AttendanceHeader";
import { PeopleRecords } from "../components/records/PeopleRecords";
import { SessionRecords } from "../components/records/SessionRecords";
import { SessionSheet } from "../components/form/SessionSheet";
import styles from "./RecordsPage.module.css";
import { RecordsFilters } from "../components/records/RecordsFilters";

type View = "sessions" | "people";
const views: readonly { value: View; label: string }[] = [
  { value: "sessions", label: "Sessions" },
  { value: "people", label: "People" },
];

// The Records tab: the numbers for the days you chose, then either the sessions held in them or everyone who was marked
// present. The view is in the address (`/attendance/records/people`), and switching keeps the filters.
export function RecordsPage() {
  const today = todayText();
  const navigate = useNavigate();
  const { pathname, search } = useLocation();
  const people = pathname === paths.attendanceRecords("people");
  const { filters, update, clear, resetKey } = useUrlFilters(recordFilters);
  const [creating, setCreating] = useState(false);
  const labels = useSessionLabels();
  const summary = useAttendanceSummary(windowDates(filters, today));
  const narrowed = isNarrowed(filters, people);

  return (
    <div className="anim-rise">
      <AttendanceHeader tab="records" onNew={() => setCreating(true)} />
      <div
        role="tabpanel"
        id="attendance-panel-records"
        aria-labelledby="attendance-tab-records"
        className={styles.panel}
      >
        <div>
          <StatStrip
            label="Attendance in this period"
            stats={presenceStats(
              summary.data,
              "In this period",
              "No one marked",
              "Present",
            )}
            busy={summary.isPending}
          />
        </div>
        <AttendanceCard today={today} title="Attendance by month" />
        <RecordsFilters
          filters={filters}
          people={people}
          labels={labels.data ?? []}
          onChange={update}
          resetKey={resetKey}
          narrowed={narrowed}
          onClear={clear}
          view={
            <SegmentedField
              label="Show"
              hideLabel
              joined
              compact
              options={views}
              value={people ? "people" : "sessions"}
              onChange={(v) =>
                navigate({
                  pathname: paths.attendanceRecords(
                    v === "people" ? "people" : undefined,
                  ),
                  search,
                })
              }
            />
          }
        />
        {people ? (
          <PeopleRecords
            query={toCheckInQuery(filters, today)}
            narrowed={narrowed}
            onClear={clear}
          />
        ) : (
          <SessionRecords
            filters={toSessionFilters(filters, today)}
            narrowed={narrowed}
          />
        )}
      </div>
      <SessionSheet
        open={creating}
        day={today}
        onClose={() => setCreating(false)}
        onCreated={(s) => navigate(paths.attendanceDay(s.date))}
      />
    </div>
  );
}
