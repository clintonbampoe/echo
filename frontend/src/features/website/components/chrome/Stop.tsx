import styles from "./Stop.module.css";

// The full stop at the end of a title. Every few seconds a thin ring spreads out from it, like an echo.
export function Stop() {
  return <span className={styles.stop}>.</span>;
}
