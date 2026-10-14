import type { ReactNode } from "react";
import type { IconName } from "@/ui";
import { AuthHeading } from "./AuthHeading";
import styles from "./StatusScreen.module.css";

type Props = {
  icon: IconName;
  title: string;
  // The sentence under the title.
  text: ReactNode;
  // Buttons and links under the text.
  children?: ReactNode;
};

// The shared shape of every "something happened" screen: check your email, verified,
// link expired, password updated. An icon, a title, a sentence, then what to do next.
export function StatusScreen({ icon, title, text, children }: Props) {
  return (
    <div role="status" aria-live="polite">
      <AuthHeading icon={icon} title={title}>
        {text}
      </AuthHeading>
      {children && <div className={styles.actions}>{children}</div>}
    </div>
  );
}
