import type { ReactNode } from "react";
import { Icon } from "../layout/Icon";
import styles from "./Notice.module.css";

type Props = {
  title: string;
  children?: ReactNode;
  // What to do about it: Buttons or links. Sits under the text.
  action?: ReactNode;
  // 'warning' is for something that will stop the save if it is ignored, and wears the warning colours and icon.
  tone?: "info" | "warning";
};

// A heads-up that is not an error: something worth knowing before carrying on, with the way to act on it.
// (An Alert is for what went wrong; a Notice is for what the person may want to look at first.)
export function Notice({ title, children, action, tone = "info" }: Props) {
  return (
    <div
      role="status"
      className={`${styles.notice} ${tone === "warning" ? styles.warning : ""}`}
    >
      {tone === "warning" && (
        <Icon name="alert" size={20} className={styles.icon} />
      )}
      <div className={styles.content}>
        <p className={styles.title}>{title}</p>
        {children && <div className={styles.body}>{children}</div>}
        {action && <div className={styles.action}>{action}</div>}
      </div>
    </div>
  );
}
