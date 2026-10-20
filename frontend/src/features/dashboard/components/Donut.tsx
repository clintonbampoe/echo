import styles from "./Donut.module.css";

export type DonutSlice = { key: string; value: number; color: string };

const R = 36;
const CIRCLE = 2 * Math.PI * R;
const GAP = 1.2;

// A ring cut into slices, biggest first, clockwise from the top. With nothing to show it is an empty ring.
export function Donut({
  label,
  slices,
}: {
  label: string;
  slices: readonly DonutSlice[];
}) {
  const total = slices.reduce((sum, s) => sum + s.value, 0);
  let start = 0;
  return (
    <svg
      viewBox="0 0 100 100"
      role="img"
      aria-label={label}
      className={styles.donut}
    >
      <circle
        cx="50"
        cy="50"
        r={R}
        fill="none"
        stroke="var(--bg)"
        strokeWidth="22"
      />
      {total > 0 &&
        slices.map((s) => {
          const length = (s.value / total) * CIRCLE;
          const at = start;
          start += length;
          return (
            <circle
              key={s.key}
              cx="50"
              cy="50"
              r={R}
              fill="none"
              stroke={s.color}
              strokeWidth="22"
              strokeDasharray={`${Math.max(0, length - (slices.length > 1 ? GAP : 0))} ${CIRCLE}`}
              strokeDashoffset={-at}
            />
          );
        })}
    </svg>
  );
}
