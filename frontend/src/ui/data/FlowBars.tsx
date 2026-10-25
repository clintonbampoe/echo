import { cssVars } from "../hooks/cssVars";
import styles from "./FlowBars.module.css";

export type FlowBar = {
  key: string;
  // The short name under the bars, e.g. "Oct".
  label: string;
  // What a screen reader says, e.g. "October 2026".
  name: string;
  income: number;
  expenses: number;
};

type Props = {
  label: string;
  bars: FlowBar[];
  // The month drawn dark.
  selected?: string;
  onPick?: (key: string) => void;
  // How a total is said, e.g. "GH₵ 21,300".
  format: (value: number) => string;
};

// Money in above a line and money out below it, one pair for each month, on one shared scale. The picked month is dark.
export function FlowBars({ label, bars, selected, onPick, format }: Props) {
  const top = Math.max(1, ...bars.flatMap((b) => [b.income, b.expenses]));
  const size = (v: number) => `${v > 0 ? Math.max(3, (v / top) * 100) : 0}%`;
  return (
    <ul aria-label={label} className={styles.bars}>
      {bars.map((b, i) => {
        const spoken = `${b.name}: income ${format(b.income)}, expenses ${format(b.expenses)}`;
        const mark = (
          <>
            <span className={`${styles.tip} glass-tip`} aria-hidden="true">
              <strong>{b.name}</strong>
              <span className={styles.line}>Income {format(b.income)}</span>
              <span className={styles.line}>Expenses {format(b.expenses)}</span>
            </span>
            <span className={styles.up}>
              <span
                className={styles.bar}
                style={cssVars({ "--h": size(b.income), "--i": i })}
              />
            </span>
            <span className={styles.down}>
              <span
                className={styles.bar}
                style={cssVars({ "--h": size(b.expenses), "--i": i })}
              />
            </span>
            <span className={styles.name} aria-hidden="true">
              {b.label}
            </span>
          </>
        );
        const picked = b.key === selected;
        return (
          <li key={b.key} className={styles.item} data-picked={picked}>
            {onPick ? (
              <button
                type="button"
                className={styles.cell}
                aria-label={spoken}
                aria-pressed={picked}
                onClick={() => onPick(b.key)}
              >
                {mark}
              </button>
            ) : (
              <div className={styles.cell} role="img" aria-label={spoken}>
                {mark}
              </div>
            )}
          </li>
        );
      })}
    </ul>
  );
}
