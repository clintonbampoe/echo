import { ProgressBar } from "@/ui";
import styles from "./BarRows.module.css";

export type BarRow = {
  key: string;
  name: string;
  // How full the bar is, 0 to 1.
  share: number;
  // The count as shown.
  text: string;
  // The same share as a percent, when the list says shares ("13%").
  sub?: string;
};

// Named bars one under the other, each with its count: a small way to compare a few things. With
// `shares`, each row also says its share as a percent, the way the age groups card does.
export function BarRows({
  label,
  rows,
  shares,
}: {
  label: string;
  rows: readonly BarRow[];
  shares?: boolean;
}) {
  return (
    <ul
      aria-label={label}
      className={shares ? `${styles.rows} ${styles.shares}` : styles.rows}
    >
      {rows.map((r) => (
        <li key={r.key} className={styles.row}>
          <span className={styles.name} title={r.name}>
            {r.name}
          </span>
          <ProgressBar value={r.share} label={r.name} />
          <span className={styles.count}>{r.text}</span>
          {shares && <span className={styles.sub}>{r.sub}</span>}
        </li>
      ))}
    </ul>
  );
}
