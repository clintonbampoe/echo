import type { ReactNode } from "react";
import styles from "./Section.module.css";

type Props = {
  children: ReactNode;
  id?: string;
  // Extra space above, in px. Landing sections breathe: most use 120.
  top?: number;
};

export function Section({ children, id, top = 120 }: Props) {
  return (
    <section id={id} className={styles.section} style={{ paddingTop: top }}>
      {children}
    </section>
  );
}
