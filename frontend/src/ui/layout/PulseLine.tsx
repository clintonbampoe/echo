import styles from "./PulseLine.module.css";

// The quiet line at the foot of the public pages: a dot that breathes, then the promise.
export function PulseLine({ className = "" }: { className?: string }) {
  return (
    <p className={`${styles.line} ${className}`}>
      <span className={styles.dot} aria-hidden="true" />
      Free for every church. Always.
    </p>
  );
}
