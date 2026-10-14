import { todayText } from "@/lib/dates";
import { firstName } from "@/lib/format";
import { Section } from "@/ui";
import { useAttendance, useRecentSessions } from "../hooks";
import { stripSlots } from "../view";
import { AttendanceStrip } from "./AttendanceStrip";
import { AttendanceTable } from "./AttendanceTable";

// How many squares a visitor's strip shows. A visitor has fewer visits than a member has Sundays.
export const VISIT_SQUARES = 10;

// Every session a visitor was marked present at: the last ten as squares, then the full list. Each opens its session.
export function PersonVisits({
  personId,
  name,
}: {
  personId: string;
  name: string;
}) {
  const today = todayText();
  const list = useAttendance(personId);
  const recent = useRecentSessions("", today, VISIT_SQUARES);
  const attended = new Set(list.rows.map((a) => a.sessionId));

  return (
    <Section
      title="Visits"
      hint={`Every session ${firstName(name)} was marked present.`}
    >
      <AttendanceStrip
        slots={stripSlots(recent.data?.data ?? [], attended)}
        size={VISIT_SQUARES}
        loading={recent.isPending || list.loading}
        failed={recent.isError || list.failed}
      />
      <AttendanceTable list={list} narrowed={false} />
    </Section>
  );
}
