import type { ReactNode } from "react";
import { Icon, type IconName } from "../layout/Icon";
import styles from "./EmptyState.module.css";

type Props = {
  icon: IconName;
  // What is missing, as a short phrase: "No attendance yet".
  title: string;
  // One or two lines on why, and where the first one comes from.
  children: ReactNode;
  // The one thing to do about it, if there is one: a Button or LinkButton.
  action?: ReactNode;
  // Smaller, for a table's place inside a page that has other content.
  compact?: boolean;
};

// A card that says nothing is here yet and what to do about it. Used wherever a list can be empty.
export function EmptyState({ icon, title, children, action, compact }: Props) {
  return (
    <div
      role="status"
      className={`${styles.card} ${compact ? styles.compact : ""}`}
    >
      <span className={styles.tile} aria-hidden="true">
        <Icon name={icon} size={compact ? 18 : 22} />
      </span>
      <h3 className={styles.title}>{title}</h3>
      <p className={styles.text}>{children}</p>
      {action && <div className={styles.action}>{action}</div>}
    </div>
  );
}
