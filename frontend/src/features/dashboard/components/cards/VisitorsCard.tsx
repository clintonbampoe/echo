import { Link } from "react-router-dom";
import { daysAfter } from "@/lib/dates";
import { formatDayTime, formatNumber } from "@/lib/format";
import { Aside, Avatar, DashCard } from "@/ui";
import { useRecentVisitors, useVisitors } from "../../hooks/people";
import styles from "./VisitorsCard.module.css";

const ROWS = 5;
const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// Visitors of the last 30 days: how many were added and how many became members, then the newest five.
// Not the period: the card always looks at the same month.
export function VisitorsCard({ today }: { today: string }) {
  const figures = useVisitors(daysAfter(today, -29), today).figures;
  const recent = useRecentVisitors(today).visitors;
  const rows = recent
    ? [...recent].sort((a, b) => b.added.localeCompare(a.added)).slice(0, ROWS)
    : undefined;
  return (
    <DashCard title="Visitors" control={<Aside>Last 30 days</Aside>}>
      <div role="group" aria-label="Last 30 days" className={styles.figures}>
        <div>
          <p className={styles.label}>New visitors</p>
          <p className={styles.value}>{num(figures?.added)}</p>
        </div>
        <div>
          <p className={styles.label}>Became members</p>
          <p className={styles.value}>{num(figures?.joined)}</p>
        </div>
      </div>
      {rows?.length === 0 ? (
        <p className={styles.empty}>No visitors yet</p>
      ) : (
        <ul aria-busy={!rows} className={styles.list}>
          {(rows ?? []).map((v) => (
            <li key={v.id}>
              <Link
                to={`/visitors/${v.id}`}
                className={`${styles.row} liquid-row`}
              >
                <Avatar name={v.name} size={36} />
                <span className={styles.text}>
                  <span className={styles.name}>{v.name}</span>
                  <span className={styles.when}>{formatDayTime(v.added)}</span>
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </DashCard>
  );
}
