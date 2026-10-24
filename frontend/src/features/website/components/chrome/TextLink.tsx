import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import styles from "./TextLink.module.css";

type Props = { to: string; children: ReactNode; external?: boolean };

// An underlined link in running text.
export function TextLink({ to, children, external }: Props) {
  return external ? (
    <a href={to} className={styles.link} target="_blank" rel="noreferrer">
      {children}
    </a>
  ) : (
    <Link to={to} className={styles.link}>
      {children}
    </Link>
  );
}
