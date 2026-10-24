import type { CSSProperties } from "react";
import { cssVars } from "./cssVars";
import styles from "./StatStrip.module.css";

// One number with a name and a line of context, e.g. "Attendance" / "86%" / "38 of 44 sessions".
export type Stat = { label: string; value: string; note: string };

type Props = {
  // Names the strip for screen readers.
  label: string;
  stats: Stat[];
  busy?: boolean;
};

// Four numbers in a row (a page with three shows three, sharing the width), and two by two on a phone. A value or note too long for its tile is cut with an ellipsis
// (the full text shows on hover), so it never runs into the next tile.
export function StatStrip({ label, stats, busy }: Props) {
  return (
    <section
      aria-label={label}
      aria-busy={busy}
      className={styles.strip}
      style={{ "--count": stats.length } as CSSProperties}
    >
      {stats.map((s, i) => (
        <div
          key={s.label}
          className={`${styles.item} stagger`}
          style={cssVars({ "--i": i })}
        >
          <p className={styles.label}>{s.label}</p>
          {busy ? (
            <span
              className={`skeleton ${styles.pending}`}
              aria-hidden="true"
            />
          ) : (
            <>
              <p className={styles.value} title={s.value}>
                {s.value}
              </p>
              <p className={styles.note} title={s.note}>
                {s.note}
              </p>
            </>
          )}
        </div>
      ))}
    </section>
  );
}
