import { Link } from "react-router-dom";
import { formatTime } from "@/lib/format";
import { paths } from "@/lib/paths";
import { DateTile } from "@/ui";
import { useAttendance } from "../hooks";
import styles from "./RecentVisits.module.css";

const SHOWN = 3;

// A person's latest few sessions, newest first, each opening its session. The full list is on their Visits tab.
export function RecentVisits({ personId }: { personId: string }) {
  const list = useAttendance(personId);
  const rows = list.rows.slice(0, SHOWN);
  return (
    <section aria-label="Recent visits" className={styles.card}>
      <h2 className={styles.title}>Recent visits</h2>
      {list.loading && <p className={styles.note}>Loading visits…</p>}
      {list.failed && <p className={styles.note}>We could not load visits.</p>}
      {!list.loading && !list.failed && rows.length === 0 && (
        <p className={styles.note}>No visits yet.</p>
      )}
      <ul className={styles.list}>
        {rows.map((a) => (
          <li key={a.id} className={styles.row}>
            <DateTile date={a.sessionDate} />
            <div>
              <Link to={paths.session(a.sessionId)} className={styles.name}>
                {a.sessionName || a.sessionLabelName}
              </Link>
              <p className={styles.time}>{formatTime(a.checkedInAt)}</p>
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
