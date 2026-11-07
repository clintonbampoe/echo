import type { ReactNode } from "react";
import { Logo } from "@/ui";
import { BackLink } from "./AuthLinks";
import { useSplit } from "./splitMode";
import { WordReveal } from "./WordReveal";
import styles from "./AuthScreen.module.css";

type Props = {
  title: string;
  // A small line over the title, e.g. a greeting.
  kicker?: string;
  // The sentence under the title.
  text?: ReactNode;
  // The other places someone on this screen might go (an AuthLinks or BackLink).
  links?: ReactNode;
  // The card: a form, or the buttons that follow a message.
  children: ReactNode;
};

// The shape of every auth screen. Wide: the words and card on the left, the other places to go
// in a glass pane on the right. Tablet and phone: one centred column, with one link under it.
export function AuthScreen({
  title,
  kicker,
  text,
  links = <BackLink />,
  children,
}: Props) {
  const split = useSplit();
  const body = (
    <>
      <Logo size={split ? 40 : 46} />
      {kicker && (
        <p className={styles.kicker}>
          <span className={styles.kickDot} aria-hidden="true" />
          {kicker}
        </p>
      )}
      <h1 className={styles.title}>
        <WordReveal text={title} />
      </h1>
      {text && <p className={styles.text}>{text}</p>}
      <div className={styles.card}>{children}</div>
    </>
  );
  if (!split) {
    return (
      <div className={styles.single}>
        {body}
        {links}
      </div>
    );
  }
  return (
    <div className={styles.split}>
      <section className={styles.formCol}>
        <div className={styles.content}>{body}</div>
      </section>
      <aside className={styles.pane}>
        <span className={styles.shine} aria-hidden="true" />
        {links}
      </aside>
    </div>
  );
}
