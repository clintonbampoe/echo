import styles from "./ChurchBorn.module.css";

// The moment a church exists: a ring closes around a check, and the name arrives.
export function ChurchBorn({ name }: { name: string }) {
  return (
    <div className={styles.born} role="status">
      <svg
        width="64"
        height="64"
        viewBox="0 0 64 64"
        fill="none"
        aria-hidden="true"
      >
        <circle
          cx="32"
          cy="32"
          r="28"
          className={styles.ring}
          pathLength="100"
        />
        <path d="M21 33l8 8 15-17" className={styles.tick} pathLength="100" />
      </svg>
      <p className={styles.name}>{name}</p>
      <p className={styles.line}>is on Echo.</p>
    </div>
  );
}
