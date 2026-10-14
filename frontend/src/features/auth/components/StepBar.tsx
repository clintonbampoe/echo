import styles from "./StepBar.module.css";

// "Step 1 of 2" with a segmented bar.
export function StepBar({ step, of }: { step: number; of: number }) {
  return (
    <div>
      <p className={styles.label}>
        Step {step} of {of}
      </p>
      <div className={styles.bar} aria-hidden="true">
        {Array.from({ length: of }, (_, i) => (
          <span key={i} className={i < step ? styles.done : styles.todo} />
        ))}
      </div>
    </div>
  );
}
