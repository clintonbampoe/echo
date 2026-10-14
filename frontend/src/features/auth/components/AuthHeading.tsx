import type { ReactNode } from "react";
import { Icon, Logo, type IconName } from "@/ui";
import styles from "./AuthHeading.module.css";

type Props = {
  title: string;
  children?: ReactNode;
  // A tile with an icon above the title; without one, the Echo logo is shown.
  icon?: IconName;
};

export function AuthHeading({ title, children, icon }: Props) {
  return (
    <div>
      {icon ? (
        <div className={styles.tile}>
          <Icon name={icon} size={26} />
        </div>
      ) : (
        <Logo />
      )}
      <h1 className={styles.title}>{title}</h1>
      {children && <p className={styles.text}>{children}</p>}
    </div>
  );
}
