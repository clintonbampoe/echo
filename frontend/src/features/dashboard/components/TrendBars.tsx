import type { CSSProperties } from "react";
import styles from "./TrendBars.module.css";

export type TrendBar = {
  key: string;
  // Under the bar, e.g. "11 Oct".
  label: string;
  // For a screen reader, e.g. "Week of 11 Oct".
  name: string;
  // The tooltip's heading, e.g. "11 – 17 Oct".
  tip: string;
  value: number;
  // The value as shown, e.g. "912".
  text: string;
  // A second bar beside the first, e.g. expenses beside income. Both share one scale.
  second?: { value: number; text: string };
};

type Legend = [string, string];

type Props = {
  label: string;
  bars: readonly TrendBar[];
  // The bar picked, drawn at full strength. Without it the last bar is the one.
  selected?: string;
  // Makes each bar a button, so the chart can also pick one.
  onPick?: (key: string) => void;
  // Names the two bars of a pair, e.g. ["Income", "Expenses"].
  legend?: Legend;
};

const TALLEST = 88;

const spoken = (b: TrendBar, legend?: Legend) =>
  b.second && legend
    ? `${b.name}: ${legend[0]} ${b.text}, ${legend[1]} ${b.second.text}`
    : `${b.name}: ${b.text}`;

type MarkProps = {
  bar: TrendBar;
  height: (value: number) => number;
  picked: boolean;
  legend?: Legend;
};

// The tooltip: its heading, then the value, or both values of a pair.
function Tip({ bar: b, legend }: { bar: TrendBar; legend?: Legend }) {
  return (
    <span className={styles.tip} aria-hidden="true">
      <strong>{b.tip}</strong>
      {b.second && legend ? (
        <>
          <span className={styles.line}>
            {legend[0]} {b.text}
          </span>
          <span className={styles.line}>
            {legend[1]} {b.second.text}
          </span>
        </>
      ) : (
        b.text
      )}
    </span>
  );
}

// The tooltip and the bar or pair of bars.
function Marks({ bar: b, height, picked, legend }: MarkProps) {
  const first = b.second ? styles.first : picked ? styles.now : "";
  return (
    <>
      <Tip bar={b} legend={legend} />
      <span
        className={`${styles.bar} ${first}`}
        style={{ height: `${height(b.value)}%` }}
      />
      {b.second && (
        <span
          className={`${styles.bar} ${styles.second}`}
          style={{ height: `${height(b.second.value)}%` }}
        />
      )}
    </>
  );
}

// One stretch: a button when the chart can pick, otherwise a picture with a spoken name.
function Item({
  onPick,
  ...mark
}: MarkProps & { onPick?: (key: string) => void }) {
  const { bar: b, height, picked, legend } = mark;
  const lift = Math.max(height(b.value), b.second ? height(b.second.value) : 0);
  const className = [
    styles.cell,
    b.second && styles.pair,
    picked && styles.picked,
  ]
    .filter(Boolean)
    .join(" ");
  const style = { "--lift": `${lift}%` } as CSSProperties;
  return (
    <li className={styles.item}>
      {onPick ? (
        <button
          type="button"
          className={className}
          style={style}
          aria-label={spoken(b, legend)}
          aria-pressed={picked}
          onClick={() => onPick(b.key)}
        >
          <Marks {...mark} />
        </button>
      ) : (
        <div
          role="img"
          aria-label={spoken(b, legend)}
          className={className}
          style={style}
        >
          <Marks {...mark} />
        </div>
      )}
    </li>
  );
}

// One bar (or pair) per stretch on a shared scale. The tallest stops short of the top, so its tooltip has
// room. On a narrow card, long labels ("23 Aug") show only at the two ends.
export function TrendBars({ label, bars, selected, onPick, legend }: Props) {
  const top = Math.max(
    0,
    ...bars.flatMap((b) => [b.value, b.second?.value ?? 0]),
  );
  const last = bars.length - 1;
  const height = (v: number) =>
    top > 0 ? Math.max(3, (v / top) * TALLEST) : 3;
  const dense = bars.some((b) => b.label.length > 4);

  return (
    <div className={styles.chart}>
      <ul aria-label={label} className={styles.bars}>
        {bars.map((b, i) => (
          <Item
            key={b.key}
            bar={b}
            height={height}
            picked={selected === undefined ? i === last : selected === b.key}
            legend={legend}
            onPick={onPick}
          />
        ))}
      </ul>
      <ol
        className={`${styles.labels} ${dense ? styles.dense : ""}`}
        aria-hidden="true"
      >
        {bars.map((b, i) => (
          <li
            key={b.key}
            className={i === 0 || i === last ? styles.end : styles.mid}
          >
            {b.label}
          </li>
        ))}
      </ol>
    </div>
  );
}
