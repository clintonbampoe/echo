import { Link } from "react-router-dom";
import { paths } from "@/lib/paths";
import { Chip } from "@/ui";
import type { Session } from "../api";
import { sessionTitle, statusTone } from "../day";
import { formatClock } from "@/lib/format";
import styles from "./SessionCard.module.css";

// The sessions of one day: when, what, how many came. The whole card opens the session.
export function SessionList({ sessions }: { sessions: Session[] }) {
  return (
    <ul aria-label="Sessions" className={styles.list}>
      {sessions.map((s) => (
        <SessionCard key={s.id} session={s} />
      ))}
    </ul>
  );
}

function SessionCard({ session: s }: { session: Session }) {
  const when = s.startsAt
    ? s.endsAt
      ? `${formatClock(s.startsAt)} – ${formatClock(s.endsAt)}`
      : formatClock(s.startsAt)
    : "No time set";
  return (
    <li className={styles.card}>
      <div className={styles.main}>
        <p className={styles.name}>
          <Link to={paths.session(s.id)} className={styles.link}>
            {sessionTitle(s)}
          </Link>
          {s.status !== "Completed" && (
            <Chip tone={statusTone[s.status]}>{s.status}</Chip>
          )}
        </p>
        <p className={styles.topic}>
          {[when, s.topic].filter(Boolean).join(" · ")}
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
