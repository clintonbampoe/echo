import type { CSSProperties, ReactNode } from "react";
import type { Part } from "./DistributionBar";
import styles from "./Donut.module.css";

type Props = {
  // The parts of the whole. A part with nothing in it is left out.
  parts: Part[];
  // The part the pointer or the keyboard is on, and the one the person picked. The parent owns both, so a key
  // beside the ring can light the same part up.
  hot: string | null;
  picked: string | null;
  onHot: (key: string | null) => void;
  onPick: (key: string) => void;
  // What sits in the hole: the total, or the figures of the active part.
  children: ReactNode;
};

const SIZE = 100;
const STROKE = 11;
const RADIUS = (SIZE - STROKE - 6) / 2;
const GAP = 3;

// A whole split into rounded arcs, with a number in the middle. It is the picture of a list the parent draws beside
// it, so the ring is hidden from screen readers. The size comes from `--donut` on any parent.
export function Donut({ parts, hot, picked, onHot, onPick, children }: Props) {
  const shown = parts.filter((p) => p.value > 0);
  const total = shown.reduce((sum, p) => sum + p.value, 0);
  const circumference = 2 * Math.PI * RADIUS;
  // Round caps reach half a stroke past each end of an arc, so the gap that shows is the gap left minus a stroke.
  const gap = STROKE + GAP;
  // Where each arc starts along the ring: the sum of the arcs before it.
  const starts = shown.map((_, i) =>
    shown
      .slice(0, i)
      .reduce((sum, p) => sum + (p.value / total) * circumference, 0),
  );
  return (
    <div className={styles.donut} data-active={hot !== null || picked !== null}>
      <svg viewBox={`0 0 ${SIZE} ${SIZE}`} aria-hidden="true">
        {shown.map((p, i) => {
          const length = Math.max((p.value / total) * circumference - gap, 0.5);
          const offset = -((starts[i] ?? 0) + gap / 2);
          return (
            <circle
              key={p.key}
              className={`${styles.arc} ${styles[`c${i % 4}`]}`}
              cx={SIZE / 2}
              cy={SIZE / 2}
              r={RADIUS}
              style={
                {
                  strokeDasharray: `${length} ${circumference}`,
                  strokeDashoffset: offset,
                } as CSSProperties
              }
              data-on={p.key === (hot ?? picked)}
              transform={`rotate(-90 ${SIZE / 2} ${SIZE / 2})`}
              onPointerEnter={(e) => {
                if (e.pointerType === "mouse") onHot(p.key);
              }}
              onPointerLeave={() => onHot(null)}
              onClick={() => onPick(p.key)}
            />
          );
        })}
      </svg>
      <div className={styles.hole}>{children}</div>
    </div>
  );
}
