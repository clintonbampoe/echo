import { Link } from "react-router-dom";
import { formatClock } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Chip, ListHeader } from "@/ui";
import type { Session } from "../../api/sessions";
import {
  dayTitle,
  sessionCount,
  sessionTitle,
  statusTone,
} from "../../model/day";
import styles from "./SessionCard.module.css";

type Props = {
  // The day the sessions were held, for the line that explains the list.
  day: string;
  sessions: Session[];
};

// The sessions of one day as the app's usual list card: a "Sessions" header, a line saying what the list is, then a row
// for each with its time tile, its name and topic, and how many came. The whole row opens the session.
export function SessionList({ day, sessions }: Props) {
  return (
    <div className={styles.card}>
      <ListHeader title="Sessions" summary={sessionCount(sessions.length)} />
      <p className={styles.help}>
        Everything held on {dayTitle(day)}. Open a session to see who came and
        mark people present.
      </p>
      <ul aria-label="Sessions" className={styles.list}>
        {sessions.map((s, i) => (
          <SessionRow key={s.id} session={s} index={i} />
        ))}
      </ul>
    </div>
  );
}

// A 56px tile with the time the session starts: "8:00" over "am". With no time set it is a dash.
function TimeTile({ startsAt }: { startsAt?: string | null }) {
  const [clock = "", period = ""] = startsAt
    ? formatClock(startsAt).split(" ")
    : [];
  return (
    <span className={`av liquid-av ${styles.tile}`} aria-hidden="true">
      <span className={styles.clock}>{clock || "–"}</span>
      <span className={styles.period}>{period}</span>
    </span>
  );
}

function SessionRow({
  session: s,
  index,
}: {
  session: Session;
  index: number;
}) {
  const when = s.startsAt
    ? s.endsAt
      ? `${formatClock(s.startsAt)} – ${formatClock(s.endsAt)}`
      : formatClock(s.startsAt)
    : "No time set";
  return (
    <li
      className={`${styles.row} liquid-row stagger`}
      style={{ "--i": Math.min(index, 12) } as React.CSSProperties}
    >
      <TimeTile startsAt={s.startsAt} />
      <div className={styles.main}>
        <p className={styles.name}>
          <Link to={paths.session(s.id)} className={styles.link}>
            {sessionTitle(s)}
          </Link>
          {s.status !== "Completed" && (
            <span
              className={s.status === "Cancelled" ? undefined : styles.quiet}
            >
              <Chip tone={statusTone[s.status]}>{s.status}</Chip>
            </span>
          )}
        </p>
        <p className={styles.topic}>
          {when}
          {s.topic && <span className={styles.extra}> · {s.topic}</span>}
        </p>
      </div>
      <p className={styles.count}>
        <span className={styles.number}>
          {s.attendanceCount.toLocaleString("en-GB")}
        </span>
        <span className={styles.unit}>present</span>
      </p>
    </li>
  );
}
