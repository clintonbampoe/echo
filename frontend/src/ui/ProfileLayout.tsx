import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { Icon } from "./Icon";
import styles from "./ProfileLayout.module.css";

// "← Members": the way back to the list a record came from.
export function BackLink({
  to,
  children,
}: {
  to: string;
  children: ReactNode;
}) {
  return (
    <Link to={to} className={styles.back}>
      <Icon name="arrow-right" size={16} className={styles.flip} />
      {children}
    </Link>
  );
}

type Props = {
  back: { to: string; label: string };
  // The card with the name and what can be done to this record.
  identity: ReactNode;
  // The details card. On a wide screen it joins the identity card; on a narrow one the page places it itself.
  aside?: ReactNode;
  // A line across the top of the page, e.g. "became a member".
  notice?: ReactNode;
  narrow: boolean;
  // The numbers and history.
  children: ReactNode;
};

// The page of one person or record: a way back, who they are, and their numbers and history. Wide, the
// identity and details are a column on the left; narrow, everything stacks.
export function ProfileLayout({
  back,
  identity,
  aside,
  notice,
  narrow,
  children,
}: Props) {
  return (
    <div className="anim-rise">
      <BackLink to={back.to}>{back.label}</BackLink>
      {notice}
      <div className={narrow ? styles.stack : styles.layout}>
        <div className={narrow ? undefined : styles.joined}>
          {identity}
          {!narrow && aside && <div>{aside}</div>}
        </div>
        <div className={styles.main}>{children}</div>
      </div>
    </div>
  );
}
