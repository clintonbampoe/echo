import styles from "./SetupBar.module.css";

type Props = { done: number; of: number };

// The thin bar of the card: how many steps are done. The fill grows from the left when a step is ticked.
export function SetupBar({ done, of }: Props) {
  return (
    <div
      role="progressbar"
      aria-label="Setup progress"
      aria-valuemin={0}
      aria-valuemax={of}
      aria-valuenow={done}
      className={styles.track}
    >
      <span
        className={styles.fill}
        style={{ width: `${(done / of) * 100}%` }}
      />
    </div>
  );
}
