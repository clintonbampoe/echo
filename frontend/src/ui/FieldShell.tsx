import type { ReactNode } from "react";
import styles from "./Field.module.css";

export type Shared = {
  label: string;
  hint?: string;
  error?: string;
  // Shown on the label row, right-aligned (e.g. "Forgot password?").
  aside?: ReactNode;
};

type ShellProps = Shared & { id: string; children: ReactNode };

// The label, hint and error that every field shares. The control goes in the middle.
export function Shell({ id, label, hint, error, aside, children }: ShellProps) {
  return (
    <div className={styles.field}>
      <div className={styles.labelRow}>
        <label id={`${id}-label`} htmlFor={id} className={styles.label}>
          {label}
        </label>
        {aside}
      </div>
      {children}
      {error ? (
        <p
          id={`${id}-msg`}
          role="alert"
          className={`${styles.message} ${styles.error} anim-fade`}
        >
          {error}
        </p>
      ) : hint ? (
        <p id={`${id}-msg`} className={styles.message}>
          {hint}
        </p>
      ) : null}
    </div>
  );
}
