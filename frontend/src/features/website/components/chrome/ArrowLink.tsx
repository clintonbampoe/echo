import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import styles from "./ArrowLink.module.css";

type Props = { to: string; children: ReactNode; external?: boolean };

// A short link under a heading or card: the words, a thin rule, an arrow that leans forward on hover.
export function ArrowLink({ to, children, external }: Props) {
  const inner = (
    <>
      {children}
      <span className={styles.arrow} aria-hidden="true">
        →
      </span>
    </>
  );
  return external ? (
    <a href={to} className={styles.link} target="_blank" rel="noreferrer">
      {inner}
    </a>
  ) : (
    <Link to={to} className={styles.link}>
      {inner}
    </Link>
  );
}
