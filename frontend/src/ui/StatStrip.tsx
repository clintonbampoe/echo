import type { CSSProperties } from "react";
import { cssVars } from "./cssVars";
import { Icon } from "./Icon";
import styles from "./StatStrip.module.css";

// One number with a name and a line of context, e.g. "Attendance" / "86%" / "38 of 44 sessions".
// `change` is a small chip beside the value, e.g. "+30%" against last year.
export type Stat = {
  label: string;
  value: string;
  note?: string;
  change?: string;
  // A small unit after the value ("%", "GHS").
  unit?: string;
};

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
          <p className={styles.label}>
            {s.label}
            {s.unit && <span className={styles.labelUnit}> · {s.unit}</span>}
          </p>
          {busy ? (
            <span className={`skeleton ${styles.pending}`} aria-hidden="true" />
          ) : (
            <div className={styles.bottom}>
              <p
                className={styles.value}
                title={[`${s.value}${s.unit ?? ""}`, s.note]
                  .filter(Boolean)
                  .join(" · ")}
              >
                {s.value}
                {s.unit && <span className={styles.unit}>{s.unit}</span>}
              </p>
              {/* The context line is not drawn (the boards leave it out); screen readers still get it. */}
              {s.note && <p className={styles.note}>{s.note}</p>}
              {s.change && (
                <span
                  className={`${styles.change} ${s.change.startsWith("−") ? styles.down : ""}`}
                >
                  <Icon
                    name={
                      s.change.startsWith("−") ? "chevron-down" : "chevron-up"
                    }
                    size={14}
                  />
                  {s.change.replace(/^[+−]/, "")}
                  <span className={styles.note}>
                    {s.change.startsWith("−") ? " down" : " up"}
                  </span>
                </span>
              )}
            </div>
          )}
        </div>
      ))}
    </section>
  );
}
