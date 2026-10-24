import type { ReactNode } from "react";
import { Ripples } from "./Ripples";
import styles from "./PageHero.module.css";

type Props = {
  // The small label above the title.
  eyebrow: string;
  // The title, one string per line.
  title: string[];
  children?: ReactNode;
  // Buttons under the text.
  actions?: ReactNode;
};

// The top of every inner page: label, big title, one sentence.
export function PageHero({ eyebrow, title, children, actions }: Props) {
  return (
    <div className={styles.hero}>
      <Ripples className={styles.ripples} />
      <span className={styles.eyebrow}>
        <span className={styles.dot} aria-hidden="true" />
        {eyebrow}
      </span>
      <h1 className={styles.title}>
        {title.map((line) => (
          <span key={line}>{line}</span>
        ))}
      </h1>
      {children && <p className={styles.lead}>{children}</p>}
      {actions && <div className={styles.actions}>{actions}</div>}
    </div>
  );
}
