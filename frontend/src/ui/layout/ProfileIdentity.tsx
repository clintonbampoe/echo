import type { ReactNode } from "react";
import { Avatar } from "./Avatar";
import styles from "./ProfileIdentity.module.css";

type Props = {
  // Names the card for screen readers, e.g. "Member".
  label: string;
  name: string;
  // Under the name: a status chip and a line such as "Member for 7 years".
  meta?: ReactNode;
  // The buttons. They share the width equally.
  children?: ReactNode;
};

// Who this record is about: an avatar, the name as the page's heading, and the things you can do.
export function ProfileIdentity({ label, name, meta, children }: Props) {
  return (
    <section
      className={styles.identity}
      aria-label={label}
      data-profile="identity"
    >
      <div className={styles.header}>
        <Avatar name={name} size={64} />
        <div className={styles.who}>
          <h1 className={styles.name}>{name}</h1>
          {meta && <p className={styles.meta}>{meta}</p>}
        </div>
      </div>
      {children && <div className={styles.actions}>{children}</div>}
    </section>
  );
}
