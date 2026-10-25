import type { ReactNode } from "react";
import { Logo } from "@/ui";
import styles from "./AuthScreen.module.css";

type Props = {
  title: string;
  // The sentence under the title.
  text?: ReactNode;
  // Plain links under the sentence: the other places someone on this screen might go.
  links?: ReactNode;
  // The card: a form, or the buttons that follow a message.
  children: ReactNode;
};

// The shape of every auth screen. The words sit on the left (on narrow screens, above) and
// the card with what to do sits on the right.
export function AuthScreen({ title, text, links, children }: Props) {
  return (
    <div className={styles.screen}>
      <div className={styles.intro}>
        <Logo size={46} />
        <h1 className={styles.title}>{title}</h1>
        {text && <p className={styles.text}>{text}</p>}
        {links}
      </div>
      <div className={styles.card}>{children}</div>
    </div>
  );
}
