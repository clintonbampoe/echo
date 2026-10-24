import type { CSSProperties, ReactNode } from "react";
import styles from "./Section.module.css";

type Props = {
  children: ReactNode;
  // Space above the section, in pixels at desktop width.
  top?: number;
  id?: string;
  className?: string;
};

// A band of the page with the site's standard width and side padding.
export function Section({ children, top = 120, id, className }: Props) {
  return (
    <section
      id={id}
      className={[styles.section, className].filter(Boolean).join(" ")}
      style={{ "--top": `${top}px` } as CSSProperties}
    >
      <div className={styles.inner}>{children}</div>
    </section>
  );
}
