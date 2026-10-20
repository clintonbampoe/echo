import { Link } from "react-router-dom";
import { donutPaths } from "./donutPaths";
import styles from "./Donut.module.css";

export type DonutSlice = {
  key: string;
  label: string;
  value: number;
  // The amount as words ("GH₵ 12,400"). Shown beside the percentage, so colour is never the only cue.
  text: string;
  // Where a click on the row goes, when it goes anywhere.
  to?: string;
};

// One cool slate tone from dark to light, largest slice first. Read as shades, with the name and number beside each.
const SHADES = ["#1F2A3A", "#4F6580", "#7F95AE", "#B3C2D3", "#DDE4EC"];

type Props = {
  label: string;
  slices: DonutSlice[];
  // Put the ring above the list instead of beside it, for a narrow card.
  stacked?: boolean;
};

// A ring split into slices, with a legend of name, amount and share for each. The legend is the real content; the ring
// only shows the shape, so it is hidden from screen readers.
export function Donut({ label, slices, stacked }: Props) {
  const total = slices.reduce((sum, s) => sum + s.value, 0);
  const shapes = donutPaths(slices);
  return (
    <div className={`${styles.wrap} ${stacked ? styles.stacked : ""}`}>
      <svg
        viewBox="0 0 140 140"
        width="150"
        height="150"
        aria-hidden="true"
        className={styles.ring}
      >
        <circle
          cx="70"
          cy="70"
          r="48"
          fill="none"
          stroke="#F4F4F3"
          strokeWidth="36"
        />
        {shapes.map((s, i) => {
          const fill = SHADES[i % SHADES.length];
          return (
            <path
              key={s.key}
              d={s.path}
              fill={fill}
              stroke={fill}
              strokeWidth="4"
              strokeLinejoin="round"
            />
          );
        })}
      </svg>
      <ul className={styles.legend} aria-label={label}>
        {slices.map((s, i) => {
          const row = (
            <>
              <span
                className={styles.swatch}
                style={{ background: SHADES[i % SHADES.length] }}
                aria-hidden="true"
              />
              <span className={styles.name}>{s.label}</span>
              <span className={styles.amount}>{s.text}</span>
              <span className={styles.share}>
                {total > 0 ? Math.round((s.value / total) * 100) : 0}%
              </span>
            </>
          );
          return (
            <li key={s.key}>
              {s.to ? (
                <Link to={s.to} className={styles.row}>
                  {row}
                </Link>
              ) : (
                <span className={styles.row}>{row}</span>
              )}
            </li>
          );
        })}
      </ul>
    </div>
  );
}
