import { formatDate, formatTime } from "@/lib/format";
import { paths } from "@/lib/paths";
import type { Paged } from "@/lib/paged";
import { RecordTable } from "@/ui";
import type { AttendanceRecord } from "../api";

type Props = { list: Paged<AttendanceRecord>; narrowed: boolean };

const everything = {
  icon: "calendar-check",
  title: "No attendance yet",
  text: "Services and meetings they are marked present at will be listed here, newest first.",
} as const;
const nothingMatches = {
  icon: "filter",
  title: "No attendance for these filters",
  text: "Try another type of session or a longer period.",
} as const;

// Every session a person was checked in to. A row opens its session.
export function AttendanceTable({ list, narrowed }: Props) {
  return (
    <RecordTable
      label="Attendance"
      list={list}
      rowKey={(a) => a.id}
      href={(a) => paths.session(a.sessionId)}
      large
      empty={narrowed ? nothingMatches : everything}
      noun={["session attended", "sessions attended"]}
      columns={[
        { header: "Session", cell: (a) => a.sessionName || a.sessionLabelName },
        { header: "Type", cell: (a) => a.sessionLabelName },
        { header: "Date", cell: (a) => formatDate(a.sessionDate) },
        {
          header: "Checked in",
          cell: (a) => (a.checkedInAt ? formatTime(a.checkedInAt) : "–"),
        },
      ]}
    />
  );
}
