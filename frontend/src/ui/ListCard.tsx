import type { ReactNode } from "react";
import styles from "./ListCard.module.css";

// One card holding a list and the search above it, as the design draws a roster: the search and filters first, then
// the rows, then the count. The list inside is drawn flat (`flat` on PersonList) so there is one edge, not two.
export function ListCard({
  toolbar,
  children,
}: {
  toolbar: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className={styles.card}>
      <div className={styles.toolbar}>{toolbar}</div>
      {children}
    </div>
  );
}
