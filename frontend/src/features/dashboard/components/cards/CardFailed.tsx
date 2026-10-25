import styles from "./CardFailed.module.css";

// What a chart card says in place of its chart when the numbers could not be loaded.
export function CardFailed({ onRetry }: { onRetry: () => void }) {
  return (
    <div className={styles.failed}>
      <p>Could not load</p>
      <button type="button" className={styles.retry} onClick={onRetry}>
        Try again
      </button>
    </div>
  );
}
