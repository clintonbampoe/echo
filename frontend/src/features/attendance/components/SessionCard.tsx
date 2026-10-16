import { Link } from "react-router-dom";
import { paths } from "@/lib/paths";
import { Chip, LinkButton } from "@/ui";
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
  return (
    <li className={styles.card}>
      <p className={styles.time}>
        {s.startsAt ? (
          formatClock(s.startsAt)
        ) : (
          <span className={styles.none}>No time set</span>
        )}
        {s.startsAt && s.endsAt && (
          <span className={styles.to}>to {formatClock(s.endsAt)}</span>
        )}
      </p>
      <div className={styles.main}>
        <p className={styles.name}>
          <Link to={paths.session(s.id)} className={styles.link}>
            {sessionTitle(s)}
          </Link>
          <Chip tone="neutral">{s.category}</Chip>
          <Chip tone={statusTone[s.status]}>{s.status}</Chip>
        </p>
        {s.topic && <p className={styles.topic}>{s.topic}</p>}
      </div>
      <p className={styles.count}>
        <span className={styles.number}>
          {s.attendanceCount.toLocaleString("en-GB")}
        </span>
        <span className={styles.unit}>present</span>
      </p>
      <LinkButton
        to={paths.session(s.id)}
        variant="secondary"
        size="sm"
        tabIndex={-1}
        aria-hidden="true"
        className={styles.open}
      >
        Open
      </LinkButton>
    </li>
  );
}
