import { ProgressBar } from "@/ui";
import styles from "./BarRows.module.css";

export type BarRow = {
  key: string;
  name: string;
  // How full the bar is, 0 to 1.
  share: number;
  // The count as shown.
  text: string;
};

// Named bars one under the other, each with its count: a small way to compare a few things.
export function BarRows({
  label,
  rows,
}: {
  label: string;
  rows: readonly BarRow[];
}) {
  return (
    <ul aria-label={label} className={styles.rows}>
      {rows.map((r) => (
        <li key={r.key} className={styles.row}>
          <span className={styles.name} title={r.name}>
            {r.name}
          </span>
          <ProgressBar value={r.share} label={r.name} />
          <span className={styles.count}>{r.text}</span>
        </li>
      ))}
    </ul>
  );
}
