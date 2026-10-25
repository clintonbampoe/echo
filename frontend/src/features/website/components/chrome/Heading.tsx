import type { ReactNode } from "react";
import styles from "./Heading.module.css";

type Props = { label?: string; title: ReactNode; children?: ReactNode };

// A section's label, big title and optional sentence.
export function Heading({ label, title, children }: Props) {
  return (
    <header className={styles.heading}>
      {label && <p className={styles.label}>{label}</p>}
      <h2 className={styles.title}>{title}</h2>
      {children && <p className={styles.lead}>{children}</p>}
    </header>
  );
}
