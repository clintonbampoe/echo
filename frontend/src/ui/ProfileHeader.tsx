import type { ReactNode } from "react";
import { Avatar } from "./Avatar";
import styles from "./ProfileHeader.module.css";

type Props = {
  // Names the header for screen readers, e.g. "Member".
  label: string;
  name: string;
  // The line under the name, e.g. "024 412 7788 · Accra · Member since Mar 2024".
  line?: string;
  // Small labels beside the name, e.g. a status chip for a member who is not active.
  badges?: ReactNode;
  // The buttons: a main action and a ⋯ menu.
  children?: ReactNode;
};

// Who a profile is about: a large avatar, the name as the page's heading, a line of facts, and what can be done.
export function ProfileHeader({ label, name, line, badges, children }: Props) {
  return (
    <section className={styles.header} aria-label={label}>
      <Avatar name={name} size={76} />
      <div className={styles.who}>
        <div className={styles.title}>
          <h1 className={styles.name}>{name}</h1>
          {badges}
        </div>
        {line && <p className={styles.line}>{line}</p>}
      </div>
      {children && <div className={styles.actions}>{children}</div>}
    </section>
  );
}
