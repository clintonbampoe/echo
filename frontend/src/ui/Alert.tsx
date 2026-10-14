import type { ReactNode } from "react";
import { Icon } from "./Icon";
import styles from "./Alert.module.css";

type Props = {
  title: string;
  children?: ReactNode;
  // An action under the text, e.g. a "Resend verification email" link.
  action?: ReactNode;
};

export function Alert({ title, children, action }: Props) {
  return (
    <div role="alert" className={`${styles.alert} anim-rise`}>
      <Icon name="alert" className={styles.icon} />
      <div>
        <p className={styles.title}>{title}</p>
        {children && <p className={styles.body}>{children}</p>}
        {action && <div className={styles.action}>{action}</div>}
      </div>
    </div>
  );
}
