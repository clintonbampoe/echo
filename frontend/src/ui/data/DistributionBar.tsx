import type { CSSProperties, ReactNode } from "react";
import { Link, useNavigate } from "react-router-dom";
import styles from "./DistributionBar.module.css";
import type { Ranked } from "./RankedBars";

// A part of the whole. `to` makes its row a link and its segment a door to that page. `share` replaces the share of the
// whole where the share means something else (a project's share of its goal); `tag` sits between the name and the figures.
export type Part = Ranked & { share?: string; tag?: ReactNode };

type Props = {
  // Names the bar for screen readers, e.g. "How it was given".
  label: string;
  // Biggest first. A part with nothing in it is left out.
  parts: Part[];
  // Said instead of the bar when there is nothing to split.
  empty?: string;
  // The part drawn in ink, when the list is filtered to it.
  selected?: string;
  // Makes each legend row a button, so a split can also be a filter. A part marked `closed` ("Other") opens nothing.
  onPick?: (key: string) => void;
};

// A whole split into parts: one slim bar of segments and a legend under it. They are one door: hover a segment and its
// legend row lights up; hover a row and its segment lifts and the others step back. The legend is the real content
// (name, amount and share), so the bar itself is hidden from screen readers.
export function DistributionBar({
  label,
  parts,
  empty,
  selected,
  onPick,
}: Props) {
  const navigate = useNavigate();
  const shown = parts.filter((p) => p.value > 0);
  const total = shown.reduce((sum, p) => sum + p.value, 0);
  const share = (p: Part) =>
    p.share ?? `${total > 0 ? Math.round((p.value / total) * 100) : 0}%`;
  if (shown.length === 0 && empty)
    return <p className={styles.empty}>{empty}</p>;
  return (
    <div className={styles.dist}>
      <div className={styles.bar} aria-hidden="true">
        {shown.map((p, i) => (
          <span
            key={p.key}
            className={`${styles.seg} ${styles[`c${i % 5}`]}`}
            style={{ flex: p.value } as CSSProperties}
            data-tip={`${p.label} · ${p.text} · ${share(p)}`}
            data-picked={p.key === selected}
            onClick={() => {
              if (p.closed) return;
              if (p.to) void navigate(p.to);
              else onPick?.(p.key);
            }}
          />
        ))}
      </div>
      <ul aria-label={label} className={styles.legend}>
        {shown.map((p, i) => {
          const row = (
            <>
              <i className={`${styles.swatch} ${styles[`c${i % 5}`]}`} />
              <span className={styles.name}>{p.label}</span>
              {p.tag}
              <span className={styles.amount}>{p.text}</span>
              <span className={styles.share}>{share(p)}</span>
            </>
          );
          return (
            <li key={p.key} className={styles.lrow}>
              {p.to && !p.closed ? (
                <Link to={p.to} className={styles.row}>
                  {row}
                </Link>
              ) : onPick && !p.closed ? (
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
