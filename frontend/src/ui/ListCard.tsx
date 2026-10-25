import type { ReactNode } from "react";
import { ListHeader } from "./ListHeader";
import styles from "./ListCard.module.css";

// One card holding a list and the search above it, as the design draws a roster: its header (what the list holds
// and a count), the search and filters, then the rows. The list inside is drawn flat (`flat` on PersonList) so
// there is one edge, not two.
export function ListCard({
  title,
  summary,
  toolbar,
  children,
}: {
  title: string;
  summary?: ReactNode;
  toolbar: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className={styles.card}>
      <ListHeader title={title} summary={summary} />
      <div className={styles.toolbar}>{toolbar}</div>
      {children}
    </div>
  );
}
