import { Link } from "react-router-dom";
import styles from "./RankedBars.module.css";

export type Ranked = {
  key: string;
  label: string;
  value: number;
  // The amount as words ("GH₵ 12k"), shown at the right of the name.
  text: string;
  // Where a click on the row goes, when it goes anywhere.
  to?: string;
  // A row that stands for several others ("Other") opens nothing: it never lists what is inside it.
  closed?: boolean;
};

type Props = {
  label: string;
  rows: Ranked[];
  // The row drawn in ink, when the list is filtered to it.
  selected?: string;
  // Makes each row a button, so a ranking can also be a filter. Choosing the selected row again clears it.
  onPick?: (key: string) => void;
};

// Things ranked from the biggest, each a name, an amount and a bar as long as its share of the biggest. Every row is a
// data door: on hover the bar grows from its centre, the others step back, and a pill gives the share. The bar only
// repeats what the words say, so it is hidden from screen readers.
export function RankedBars({ label, rows, selected, onPick }: Props) {
  const top = Math.max(...rows.map((r) => r.value), 0);
  const total = rows.reduce((sum, r) => sum + r.value, 0);
  return (
    <ul aria-label={label} className={styles.list}>
      {rows.map((r, i) => {
        const share = total > 0 ? Math.round((r.value / total) * 100) : 0;
        const picked = selected === r.key;
        const body = (
          <>
            <span className={`${styles.tip} glass-tip`} aria-hidden="true">
              {r.label} · {r.text} · {share}%
            </span>
            <span className={styles.line}>
              <span className={styles.name}>{r.label}</span>
              <strong className={styles.amount}>{r.text}</strong>
            </span>
            <span aria-hidden="true" className={styles.track}>
              <span
                className={`${styles.bar} ${i === 0 || picked ? styles.first : ""}`}
                style={{
                  width: `${top > 0 ? Math.max(3, (r.value / top) * 100) : 0}%`,
                  ["--i" as string]: i,
                }}
              />
            </span>
          </>
        );
        return (
          <li key={r.key}>
            {onPick && !r.closed ? (
              <button
                type="button"
                className={styles.row}
                aria-pressed={picked}
                onClick={() => onPick(r.key)}
              >
                {body}
              </button>
            ) : r.to ? (
              <Link to={r.to} className={styles.row}>
                {body}
              </Link>
            ) : (
              <div className={styles.row}>{body}</div>
            )}
          </li>
        );
      })}
    </ul>
  );
}
