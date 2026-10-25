import { useState } from "react";
import { todayText } from "@/lib/dates";
import {
  useAttendance,
  useRecentSessions,
  useSessionLabels,
} from "../../hooks/queries";
import {
  filtersFor,
  isNarrowed,
  stripSlots,
  type Period,
} from "../../model/display";
import { AttendanceFilters } from "../day/AttendanceFilters";
import { AttendanceStrip } from "../day/AttendanceStrip";
import { AttendanceTable } from "../day/AttendanceTable";

// Everything about when one person came: the filters, the strip of recent sessions, and the full list.
export function PersonAttendance({ personId }: { personId: string }) {
  const today = todayText();
  const [type, setType] = useState("");
  const [period, setPeriod] = useState<Period>("all");
  const list = useAttendance(personId, filtersFor(type, period, today));
  const labels = useSessionLabels();
  const recent = useRecentSessions(type, today);
  // The squares need to know where they were, whatever the period; the type is the same as the list's.
  const strip = useAttendance(personId, filtersFor(type, "all", today));
  const attended = new Set(strip.rows.map((a) => a.sessionId));
  const typeName = labels.data?.find((l) => String(l.id) === type)?.name;

  return (
    <>
      <AttendanceFilters
        labels={labels.data ?? []}
        type={type}
        period={period}
        onType={setType}
        onPeriod={setPeriod}
      />
      <AttendanceStrip
        slots={stripSlots(recent.data?.data ?? [], attended)}
        typeName={typeName}
        loading={recent.isPending || strip.loading}
        failed={recent.isError || strip.failed}
      />
      <AttendanceTable list={list} narrowed={isNarrowed(type, period)} />
    </>
  );
}
