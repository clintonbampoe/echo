import type { ReactNode } from "react";
import styles from "./PageHeader.module.css";

type Props = {
  title: string;
  // One plain line saying what the page is for.
  text: string;
  // The page's main button, e.g. "New member", or two buttons side by side (the main one last). Full width on a phone.
  action?: ReactNode;
};

// The top of a page: its name, a line about it, and the one thing you can do from here.
export function PageHeader({ title, text, action }: Props) {
  return (
    <header className={styles.header}>
      <div>
        <h1 className={styles.title}>{title}</h1>
        <p className={styles.text}>{text}</p>
      </div>
      {action && <div className={styles.action}>{action}</div>}
    </header>
  );
}
