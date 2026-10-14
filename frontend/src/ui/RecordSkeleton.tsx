import type { CSSProperties } from "react";
import styles from "./RecordTable.module.css";

// Grey bars where the rows will be, so the page does not jump when the data arrives.
export function RecordSkeleton({
  label,
  columns,
}: {
  label: string;
  columns: number;
}) {
  const cols = { "--cols": columns } as CSSProperties;
  return (
    <div
      className={styles.wrap}
      role="status"
      aria-busy="true"
      aria-label={`Loading ${label.toLowerCase()}`}
    >
      <div className={styles.head} aria-hidden="true" style={cols}>
        {Array.from({ length: columns }, (_, i) => (
          <span key={i} className={`${styles.bar} ${styles.barHead}`} />
        ))}
      </div>
      <ul className={styles.list} aria-hidden="true">
        {[0, 1, 2, 3].map((n) => (
          <li key={n} className={styles.row} style={cols}>
            {Array.from({ length: columns }, (_, i) => (
              <span
                key={i}
                className={styles.bar}
                style={{ width: i === 0 ? "70%" : "55%" }}
              />
            ))}
          </li>
        ))}
      </ul>
    </div>
  );
}
