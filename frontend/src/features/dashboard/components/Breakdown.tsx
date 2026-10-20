import { Donut } from "./Donut";
import styles from "./Breakdown.module.css";

// One slice of the ring: its size, and how it reads in the list.
export type BreakdownSlice = {
  key: string;
  name: string;
  value: number;
  // The amount as shown: "GH₵ 21,600".
  text: string;
  percent: number;
  color: string;
};

type Props = {
  // Names the ring for screen readers.
  label: string;
  // The whole, as shown, and what it is called: "Total income".
  total: string;
  totalLabel: string;
  // A narrow card puts the total above the ring with this line under it. Without it, nothing goes above.
  note?: string;
  slices: readonly BreakdownSlice[];
  ready: boolean;
  // What the list says when there is nothing to show.
  empty: string;
};

// A ring and the list that explains it. A narrow card shows each name with its percent; a wide one adds the
// amount, and puts the total at the top of the list.
export function Breakdown({
  label,
  total,
  totalLabel,
  note,
  slices,
  ready,
  empty,
}: Props) {
  return (
    <div className={styles.content} aria-busy={!ready}>
      {note && (
        <div className={styles.figure}>
          <p className={styles.big}>{total}</p>
          <p className={styles.note}>{note}</p>
        </div>
      )}
      <div className={styles.row}>
        <Donut
          label={label}
          slices={slices.map((s) => ({
            key: s.key,
            value: s.value,
            color: s.color,
          }))}
        />
        <div className={styles.legend}>
          <p className={styles.tot}>
            <span>{totalLabel}</span>
            <strong>{total}</strong>
          </p>
          {ready && slices.length === 0 ? (
            <p className={styles.empty}>{empty}</p>
          ) : (
            <ul className={styles.rows}>
              {slices.map((s) => (
                <li key={s.key} className={styles.item}>
                  <i className={styles.dot} style={{ background: s.color }} />
                  <span className={styles.name} title={s.name}>
                    {s.name}
                  </span>
                  <span className={styles.amount}>{s.text}</span>
                  <span className={styles.percent}>{s.percent}%</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}
