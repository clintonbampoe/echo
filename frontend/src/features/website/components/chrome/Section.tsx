import type { CSSProperties, ReactNode } from "react";
import styles from "./Section.module.css";

type Props = {
  children: ReactNode;
  // Space above the section, in pixels at desktop width. A tablet gets 30 less; a phone gets 40% of it.
  top?: number;
  // Space above on a tablet, when it is not 30 less than `top`.
  tablet?: number;
  // Space above on a phone, when it is not 40% of `top` (the first section clears the header).
  phone?: number;
  id?: string;
  className?: string;
};

// A band of the page with the site's standard width and side padding.
export function Section({
  children,
  top = 120,
  tablet,
  phone,
  id,
  className,
}: Props) {
  return (
    <section
      id={id}
      className={[styles.section, className].filter(Boolean).join(" ")}
      style={
        {
          "--top": `${top}px`,
          ...(tablet !== undefined && { "--top-tablet": `${tablet}px` }),
          ...(phone !== undefined && { "--top-phone": `${phone}px` }),
        } as CSSProperties
      }
    >
      <div className={styles.inner}>{children}</div>
    </section>
  );
}
