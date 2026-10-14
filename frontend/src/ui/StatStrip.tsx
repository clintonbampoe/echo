import styles from "./StatStrip.module.css";

// One number with a name and a line of context, e.g. "Attendance" / "86%" / "38 of 44 sessions".
export type Stat = { label: string; value: string; note: string };

type Props = {
  // Names the strip for screen readers.
  label: string;
  stats: Stat[];
  busy?: boolean;
};

// Four numbers in a row (two by two on a phone).
export function StatStrip({ label, stats, busy }: Props) {
  return (
    <section aria-label={label} aria-busy={busy} className={styles.strip}>
      {stats.map((s) => (
        <div key={s.label} className={styles.item}>
          <p className={styles.label}>{s.label}</p>
          <p className={styles.value}>{s.value}</p>
          <p className={styles.note}>{s.note}</p>
        </div>
      ))}
    </section>
  );
}
