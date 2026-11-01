import type { ReactNode } from "react";
import type { IconName } from "../layout/Icon";
import { StatusTile } from "../layout/StatusTile";
import styles from "./ScreenMessage.module.css";

type Props = {
  icon: IconName;
  // Red icon, for something that broke rather than something that is missing.
  tone?: "neg";
  title: string;
  children: ReactNode;
  // The ways out: a primary button first, then a quiet one.
  actions?: ReactNode;
  // Above the title, e.g. the logo on a page of its own.
  top?: ReactNode;
};

// A whole-screen message: one icon, a title, a line of help and the ways out. Used for a page that is gone, a
// page the person may not open, and a page that broke. Centred in whatever holds it.
export function ScreenMessage({
  icon,
  tone,
  title,
  children,
  actions,
  top,
}: Props) {
  return (
    <section role="alert" className={`${styles.wrap} anim-rise`}>
      {top}
      <StatusTile icon={icon} tone={tone} size={64} />
      <h1 className={styles.title}>{title}</h1>
      <p className={styles.text}>{children}</p>
      {actions && <div className={styles.actions}>{actions}</div>}
    </section>
  );
}
