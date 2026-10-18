import type { ReactNode } from "react";
import { Avatar } from "./Avatar";
import { LinkButton } from "./Button";
import styles from "./PanelIdentity.module.css";

type Props = {
  name: string;
  // Under the name: a role chip, a phone number, a line of context.
  meta?: ReactNode;
  // Their member page, on the tab for the record this panel is about (design rule S6).
  profileHref: string;
};

// Who a context panel is about: the person, and the one deliberate way out to their own page.
// Every panel that shows a person inside another record starts with this, so they all open the same way.
export function PanelIdentity({ name, meta, profileHref }: Props) {
  return (
    <div className={styles.who}>
      <Avatar name={name} size={48} />
      <div className={styles.names}>
        <p className={styles.name}>{name}</p>
        {meta}
      </div>
      <LinkButton to={profileHref} variant="secondary" size="sm">
        View profile
      </LinkButton>
    </div>
  );
}
