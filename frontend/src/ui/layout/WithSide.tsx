import type { ReactNode } from "react";
import styles from "./WithSide.module.css";

type Props = {
  children: ReactNode;
  // The card beside the list. It is dropped on a tablet and a phone: less, not smaller.
  side: ReactNode;
};

// A list with a ranked card beside it: eight columns and four of a twelve column page. Below a wide screen the list
// has the page to itself and the card goes, because the same numbers are in the chart and the list.
export function WithSide({ children, side }: Props) {
  return (
    <div className={styles.split}>
      <div className={styles.main}>{children}</div>
      <aside className={styles.side}>{side}</aside>
    </div>
  );
}
