import type { Stat } from "@/ui";
import styles from "./ContributionSummary.module.css";

// The numbers of the contributions in view. Like StatStrip, but a value is never cut off: "Mobile money" wraps.
export function ContributionSummary({
  stats,
  busy,
}: {
  stats: Stat[];
  busy: boolean;
}) {
  return (
    <section
      aria-label="Contributions summary"
      aria-busy={busy}
      className={styles.strip}
    >
      {stats.map((s, i) => (
        <div
          key={s.label}
          className={`${styles.item} ${i === stats.length - 1 ? styles.wide : ""}`}
        >
          <p className={styles.label}>{s.label}</p>
          <p className={styles.value}>{s.value}</p>
          <p className={styles.note}>{s.note}</p>
        </div>
      ))}
    </section>
  );
}
