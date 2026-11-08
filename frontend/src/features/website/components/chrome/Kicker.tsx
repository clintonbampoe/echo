import styles from "./Kicker.module.css";

// The small line over a heading. The dot grows into place as it scrolls into view.
export function Kicker({ children }: { children: string }) {
  return (
    <p className={styles.kicker}>
      <span className={styles.dot} aria-hidden="true" />
      {children}
    </p>
  );
}
