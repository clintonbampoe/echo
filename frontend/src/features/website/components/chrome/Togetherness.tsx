import styles from "./Togetherness.module.css";

// Twelve people in an unbroken ring around the Echo mark. It is the whole idea of Echo drawn once:
// many people, one place. The ring turns very slowly, and one quiet ripple goes out from the centre
// to the ring. Decorative only, so it is hidden from assistive technology.
const OPACITY = [0.8, 0.55, 0.7, 0.9, 0.5, 0.75, 0.6, 0.85, 0.5, 0.7, 0.9, 0.6];

export function Togetherness() {
  return (
    <svg
      className={styles.art}
      viewBox="0 0 520 520"
      fill="none"
      aria-hidden="true"
    >
      <circle cx="260" cy="260" r="130" fill="currentColor" fillOpacity=".07" />
      <g className={styles.ring}>
        <circle
          cx="260"
          cy="260"
          r="152"
          stroke="currentColor"
          strokeOpacity=".4"
          strokeWidth="1.2"
        />
        {OPACITY.map((opacity, i) => (
          <g
            key={i}
            transform={`rotate(${i * 30} 260 260)`}
            fill="currentColor"
            opacity={opacity}
          >
            <circle cx="260" cy="84" r="6.5" />
            <path d="M248 108a12 12 0 0124 0z" />
          </g>
        ))}
      </g>
      <circle
        className={styles.ripple}
        cx="260"
        cy="260"
        r="22"
        stroke="currentColor"
        strokeWidth="1.2"
        vectorEffect="non-scaling-stroke"
      />
      <circle
        cx="260"
        cy="260"
        r="22"
        stroke="currentColor"
        strokeWidth="1.5"
      />
      <circle cx="260" cy="260" r="6" fill="currentColor" />
    </svg>
  );
}
