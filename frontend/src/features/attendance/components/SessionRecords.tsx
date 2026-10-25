import { paths } from "@/lib/paths";
import { Chip, DateTile, RecordTable } from "@/ui";
import type { SessionFilters } from "../api";
import { sessionTitle, statusTone } from "../day";
import { useRecordSessions } from "../hooks";
import styles from "./SessionRecords.module.css";

type Props = { filters: SessionFilters; narrowed: boolean };

const everything = {
  icon: "calendar-check",
  title: "No sessions in this period",
  text: "Sessions held in the days you chose will be listed here. Try a longer period.",
} as const;
const nothingMatches = {
  icon: "filter",
  title: "No sessions match",
  text: "Try a different search, another type or status, or a longer period.",
} as const;

// The sessions in the period, with how many came. A row opens its session.
export function SessionRecords({ filters, narrowed }: Props) {
  const list = useRecordSessions(filters);
  return (
    <RecordTable
      label="Sessions"
      list={list}
      rowKey={(s) => s.id}
      href={(s) => paths.session(s.id)}
      large
      empty={narrowed ? nothingMatches : everything}
      noun={["session", "sessions"]}
      columns={[{ header: "Session", cell: sessionTitle }]}
      line={(s) => ({
        lead: <DateTile date={s.date} />,
        title: sessionTitle(s),
        sub: (
          <>
            {s.sessionLabelName}
            {s.status !== "Completed" && (
              <>
                {" · "}
                <Chip tone={statusTone[s.status]}>{s.status}</Chip>
              </>
            )}
          </>
        ),
        end: <strong className={styles.count}>{s.attendanceCount}</strong>,
      })}
    />
  );
}
