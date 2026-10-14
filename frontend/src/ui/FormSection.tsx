import type { ReactNode } from "react";
import styles from "./FormSection.module.css";

// A titled group of fields, laid out in two columns (one on a phone). Forms are sections, not one long list.
// Put `className={...}` on a child with grid-column: 1 / -1 when a field needs the full width.
export function FormSection({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <section className={styles.section}>
      <h3 className={styles.heading}>{title}</h3>
      <div className={styles.grid}>{children}</div>
    </section>
  );
}
