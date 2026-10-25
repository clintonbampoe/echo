import type { CSSProperties } from "react";
import styles from "./DistributionBar.module.css";
import type { Ranked } from "./RankedBars";

type Props = {
  // Names the bar for screen readers, e.g. "How it was given".
  label: string;
  // Biggest first. A part with nothing in it is left out.
  parts: Ranked[];
  // The part drawn in ink, when the list is filtered to it.
  selected?: string;
  // Makes each legend row a button, so a split can also be a filter. A part marked `closed` ("Other") opens nothing.
  onPick?: (key: string) => void;
};

// A whole split into parts: one slim bar of segments and a legend under it. They are one door: hover a segment and its
// legend row lights up; hover a row and its segment lifts and the others step back. The legend is the real content
// (name, amount and share), so the bar itself is hidden from screen readers.
export function DistributionBar({ label, parts, selected, onPick }: Props) {
  const shown = parts.filter((p) => p.value > 0);
  const total = shown.reduce((sum, p) => sum + p.value, 0);
  const share = (p: Ranked) =>
    total > 0 ? Math.round((p.value / total) * 100) : 0;
  return (
    <div role="group" aria-label={label} className={styles.dist}>
      <div className={styles.bar} aria-hidden="true">
        {shown.map((p, i) => (
          <span
            key={p.key}
            className={`${styles.seg} ${styles[`c${i % 5}`]}`}
            style={{ flex: p.value } as CSSProperties}
            data-tip={`${p.label} · ${p.text} · ${share(p)}%`}
            data-picked={p.key === selected}
            onClick={() => !p.closed && onPick?.(p.key)}
          />
        ))}
      </div>
      <ul className={styles.legend}>
        {shown.map((p, i) => {
          const row = (
            <>
              <i className={`${styles.swatch} ${styles[`c${i % 5}`]}`} />
              <span className={styles.name}>{p.label}</span>
              <span className={styles.amount}>{p.text}</span>
              <span className={styles.share}>{share(p)}%</span>
            </>
          );
          return (
            <li key={p.key} className={styles.lrow}>
              {onPick && !p.closed ? (
                <button
                  type="button"
                  className={styles.row}
                  aria-pressed={p.key === selected}
                  onClick={() => onPick(p.key)}
                >
                  {row}
                </button>
              ) : (
                <div className={styles.row}>{row}</div>
              )}
            </li>
          );
        })}
      </ul>
    </div>
  );
}
