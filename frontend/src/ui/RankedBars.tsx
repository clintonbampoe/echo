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
};

type Props = { label: string; rows: Ranked[] };

// Things ranked from the biggest, each a name, an amount and a bar as long as its share of the biggest. The bar only
// repeats what the words say, so it is hidden from screen readers.
export function RankedBars({ label, rows }: Props) {
  const top = Math.max(...rows.map((r) => r.value), 0);
  return (
    <ul aria-label={label} className={styles.list}>
      {rows.map((r, i) => {
        const body = (
          <>
            <span className={styles.line}>
              <span className={styles.name}>{r.label}</span>
              <strong className={styles.amount}>{r.text}</strong>
            </span>
            <span aria-hidden="true" className={styles.track}>
              <span
                className={`${styles.bar} ${i === 0 ? styles.first : ""}`}
                style={{
                  width: `${top > 0 ? Math.max(3, (r.value / top) * 100) : 0}%`,
                }}
              />
            </span>
          </>
        );
        return (
          <li key={r.key}>
            {r.to ? (
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
