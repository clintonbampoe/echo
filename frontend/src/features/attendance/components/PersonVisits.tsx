import { todayText } from "@/lib/dates";
import { firstName } from "@/lib/format";
import { Section } from "@/ui";
import { useAttendance, useRecentSessions } from "../hooks";
import { stripSlots } from "../view";
import { AttendanceStrip } from "./AttendanceStrip";
import { AttendanceTable } from "./AttendanceTable";

// Every session a visitor was marked present at: the latest sessions as squares, then the full list. Each opens its session.
export function PersonVisits({
  personId,
  name,
}: {
  personId: string;
  name: string;
}) {
  const today = todayText();
  const list = useAttendance(personId);
  const recent = useRecentSessions("", today);
  const attended = new Set(list.rows.map((a) => a.sessionId));

  return (
    <Section
      title="Visits"
      hint={`Every session ${firstName(name)} was marked present.`}
    >
      <AttendanceStrip
        slots={stripSlots(recent.data?.data ?? [], attended)}
        loading={recent.isPending || list.loading}
        failed={recent.isError || list.failed}
      />
      <AttendanceTable list={list} narrowed={false} />
    </Section>
  );
}
