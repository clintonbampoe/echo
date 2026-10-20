import styles from "./Donut.module.css";

export type DonutSlice = { key: string; value: number; color: string };

const R = 36;
const CIRCLE = 2 * Math.PI * R;
const GAP = 1.2;

type Props = {
  label: string;
  slices: readonly DonutSlice[];
  // The slice being read (hovered or picked): the rest fade so the eye can follow it round the ring.
  active?: string | null;
  // The ring answers the pointer: a hover reads a slice, a click picks it. The list is the keyboard's way in.
  onHover?: (key: string | null) => void;
  onPick?: (key: string) => void;
  // A tall card with its legend below can give the ring more room.
  big?: boolean;
};

// A ring cut into slices, biggest first, clockwise from the top. With nothing to show it is an empty ring.
// Unlike `ui`'s Donut this one draws the ring only — its caller (Breakdown) owns the legend, so the
// dashboard can cut the rows its own way and let the reader, not the ring, carry the reading.
export function Donut({
  label,
  slices,
  active = null,
  onHover,
  onPick,
  big = false,
}: Props) {
  const total = slices.reduce((sum, s) => sum + s.value, 0);
  let start = 0;
  return (
    <svg
      viewBox="0 0 100 100"
      role="img"
      aria-label={label}
      className={`${styles.donut} ${big ? styles.big : ""}`}
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
              className={styles.slice}
              style={{
                opacity: active && active !== s.key ? 0.32 : 1,
                cursor: onPick ? "pointer" : undefined,
              }}
              onPointerEnter={onHover ? () => onHover(s.key) : undefined}
              onPointerLeave={onHover ? () => onHover(null) : undefined}
              onClick={onPick ? () => onPick(s.key) : undefined}
            />
          );
        })}
    </svg>
  );
}
