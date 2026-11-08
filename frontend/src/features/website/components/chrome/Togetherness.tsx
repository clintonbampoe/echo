import styles from "./Togetherness.module.css";

// Twelve small figures in a ring around one point. It is the whole idea of Echo drawn once: many
// people, one place. Decorative only, so it is hidden from assistive technology.
const FIGURES = Array.from({ length: 12 }, (_, i) => i);

export function Togetherness() {
  return (
    <svg
      className={styles.art}
      viewBox="0 0 520 520"
      fill="currentColor"
      aria-hidden="true"
    >
      <circle cx="260" cy="260" r="130" opacity=".07" />
      <g className={styles.ring}>
        {FIGURES.map((i) => (
          <g
            key={i}
            transform={`rotate(${i * 30} 260 260)`}
            opacity={i % 3 === 0 ? 0.9 : i % 3 === 1 ? 0.55 : 0.3}
          >
            <circle cx="260" cy="84" r="6.5" />
            <path d="M248 108a12 12 0 0124 0z" />
          </g>
        ))}
      </g>
      <circle
        cx="260"
        cy="260"
        r="152"
        fill="none"
        stroke="currentColor"
        strokeWidth="1"
        opacity=".18"
      />
      <circle className={styles.pulse} cx="260" cy="260" r="6" />
      <circle
        cx="260"
        cy="260"
        r="22"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.5"
        opacity=".5"
      />
      <circle cx="260" cy="260" r="6" />
    </svg>
  );
}
