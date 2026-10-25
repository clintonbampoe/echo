import { cssVars } from "../hooks/cssVars";
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
  // A second bar beside the first, e.g. expenses against income. Both share one scale.
  compare?: { value: number; text: string };
};

type Props = {
  // Names the chart for screen readers.
  label: string;
  bars: MonthBar[];
  // The bar drawn in black.
  selected?: string;
  // Makes each bar a button.
  onPick?: (key: string) => void;
  // Names the two bars when every month carries a pair, e.g. ['Income', 'Expenses'].
  legend?: [string, string];
};

// What a screen reader hears for one month: one value, or both when the month carries a pair.
const spoken = (b: MonthBar, legend?: [string, string]) =>
  b.compare && legend
    ? `${b.name}: ${legend[0]} ${b.text}, ${legend[1]} ${b.compare.text}`
    : `${b.name}: ${b.text}`;

type MarkProps = {
  bar: MonthBar;
  // Which month, left to right, so the bars rise one after another.
  index: number;
  top: number;
  picked: boolean;
  legend?: [string, string];
};

// One month: the tooltip, the bar or pair of bars, and the month's name under them.
function Mark({ bar: b, index, top, picked, legend }: MarkProps) {
  const height = (value: number) =>
    top > 0 ? Math.max(3, (value / top) * 100) : 3;
  // In a pair the first bar is the one the legend calls solid; on its own it is solid only when picked.
  const lead = b.compare ? true : picked;
  // The tooltip rides just above the taller bar, so a full-height month never covers the chart's heading.
  const lift = cssVars({
    "--lift": `${Math.max(height(b.value), b.compare ? height(b.compare.value) : 0)}%`,
    "--i": index,
  });
  return (
    <>
      <span
        className={`${styles.track} ${b.compare ? styles.pair : ""}`}
        style={lift}
      >
        <span className={`${styles.tip} glass-tip`} aria-hidden="true">
          <strong>{b.tip}</strong>
          {b.compare && legend ? (
            <>
              <span className={styles.line}>
                {legend[0]} {b.text}
              </span>
              <span className={styles.line}>
                {legend[1]} {b.compare.text}
              </span>
            </>
          ) : (
            b.text
          )}
        </span>
        <span
          className={`${styles.bar} grow-y ${lead ? styles.on : ""}`}
          style={{ height: `${height(b.value)}%` }}
        />
        {b.compare && (
          <span
            className={`${styles.bar} grow-y ${styles.second}`}
            style={{ height: `${height(b.compare.value)}%` }}
          />
        )}
      </span>
      <span className={`${styles.label} ${picked ? styles.current : ""}`}>
        {b.label}
      </span>
    </>
  );
}

// One bar per month. Bars share one scale, start at the baseline and carry their value in a tooltip on hover
// and keyboard focus. With `onPick` a bar is a button, so a chart can also be a filter.
export function MonthBars({ label, bars, selected, onPick, legend }: Props) {
  const top = Math.max(
    0,
    ...bars.flatMap((b) => [b.value, b.compare?.value ?? 0]),
  );
  return (
    <ul aria-label={label} className={styles.bars}>
      {bars.map((b, i) => {
        const picked = selected === b.key;
        const mark = (
          <Mark bar={b} index={i} top={top} picked={picked} legend={legend} />
        );
        return (
          <li key={b.key} className={styles.item}>
            {onPick ? (
              <button
                type="button"
                className={styles.cell}
                aria-label={spoken(b, legend)}
                aria-pressed={picked}
                onClick={() => onPick(b.key)}
              >
                {mark}
              </button>
            ) : (
              <div
                className={styles.cell}
                role="img"
                aria-label={spoken(b, legend)}
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
