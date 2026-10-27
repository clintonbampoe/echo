import { Fragment, type ReactNode } from "react";
import { Link } from "react-router-dom";
import styles from "./AuthLinks.module.css";

type Item = { to: string; label: string };

// The other places someone on this screen might want to go, as underlined links on one line.
// Plain statements, not questions: "Create a church account", not "New to Echo?".
export function AuthLinks({ items }: { items: Item[] }) {
  return (
    <nav aria-label="Other options" className={styles.links}>
      {items.map((item, i) => (
        <Fragment key={item.to}>
          {i > 0 && <span aria-hidden="true">·</span>}
          <Link to={item.to} className={styles.link}>
            {item.label}
          </Link>
        </Fragment>
      ))}
    </nav>
  );
}

// The "Back to sign in" link, shown under a screen's words.
export function BackLink({
  to = "/signin",
  children = "Back to sign in",
}: {
  to?: string;
  children?: ReactNode;
}) {
  return (
    <p className={styles.back}>
      <Link to={to} className={styles.link}>
        {children}
      </Link>
    </p>
  );
}
