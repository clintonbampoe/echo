import styles from "./MonthBars.module.css";

export type MonthBar = {
  key: string;
  // The short name under the bar, e.g. "Oct".
  label: string;
  // What a screen reader says, e.g. "October 2026".
  name: string;
  // The tooltip's heading, e.g. "Oct 2026".
  tip: string;
  value: number;
  // The value as shown, e.g. "GH₵ 21,300".
  text: string;
};

type Props = {
  // Names the chart for screen readers.
  label: string;
  bars: MonthBar[];
  // The bar drawn in black.
  selected?: string;
  // Makes each bar a button.
  onPick?: (key: string) => void;
};

// One bar per month. Bars share one scale, start at the baseline and carry their value in a tooltip on hover
// and keyboard focus. With `onPick` a bar is a button, so a chart can also be a filter.
export function MonthBars({ label, bars, selected, onPick }: Props) {
  const top = Math.max(0, ...bars.map((b) => b.value));
  return (
    <ul aria-label={label} className={styles.bars}>
      {bars.map((b) => {
        const height = top > 0 ? Math.max(3, (b.value / top) * 100) : 3;
        const mark = (
          <>
            <span className={styles.tip} aria-hidden="true">
              <strong>{b.tip}</strong>
              {b.text}
            </span>
            <span className={styles.track}>
              <span
                className={`${styles.bar} ${selected === b.key ? styles.on : ""}`}
                style={{ height: `${height}%` }}
              />
            </span>
            <span
              className={`${styles.label} ${selected === b.key ? styles.current : ""}`}
            >
              {b.label}
            </span>
          </>
        );
        return (
          <li key={b.key} className={styles.item}>
            {onPick ? (
              <button
                type="button"
                className={styles.cell}
                aria-label={`${b.name}: ${b.text}`}
                aria-pressed={selected === b.key}
                onClick={() => onPick(b.key)}
              >
                {mark}
              </button>
            ) : (
              <div
                className={styles.cell}
                role="img"
                aria-label={`${b.name}: ${b.text}`}
              >
                {mark}
              </div>
            )}
          </li>
        );
      })}
    </ul>
  );
}
