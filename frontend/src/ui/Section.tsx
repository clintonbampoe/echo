import { useId, type ReactNode } from "react";
import styles from "./Section.module.css";

type Props = { title?: string; hint?: string; children: ReactNode };

// A titled block on a page, with a line of plain words under the title saying what is in it.
// Without a title it is just the line, which is how a tab with one table introduces it.
export function Section({ title, hint, children }: Props) {
  const id = useId();
  return (
    <section
      aria-labelledby={title ? id : undefined}
      className={styles.section}
    >
      {title && (
        <h3 id={id} className={styles.title}>
          {title}
        </h3>
      )}
      {hint && <p className={styles.hint}>{hint}</p>}
      {children}
    </section>
  );
}
