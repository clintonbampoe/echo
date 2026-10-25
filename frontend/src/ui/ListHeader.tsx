import type { ReactNode } from "react";
import styles from "./ListHeader.module.css";

// The line every list card opens with: what the list holds on the left, a count or short summary on the right.
// Static text: it has no hover, press or focus. Actions that belong to the card go in the page header or the
// filter row, never here. Pages do not build this themselves; the list components draw it from `title`.
export function ListHeader({
  title,
  summary,
}: {
  title: string;
  summary?: ReactNode;
}) {
  return (
    <div className={styles.header}>
      <h2 className={styles.title}>{title}</h2>
      {summary != null && summary !== "" && (
        <span className={styles.summary}>{summary}</span>
      )}
    </div>
  );
}
