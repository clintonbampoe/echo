import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { Logo, PulseLine } from "@/ui";
import { BackToSite } from "./BackToSite";
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
      {!split && <Logo size={46} />}
      {kicker && (
        <p className={styles.kicker}>
          <span className={styles.kickDot} aria-hidden="true" />
          <span key={kicker} className="anim-rise">
            {kicker}
          </span>
        </p>
      )}
      <h1
        className={styles.title}
        data-vt="title"
        data-end={/[.?!]$/.test(title) ? "punct" : undefined}
      >
        <WordReveal text={title} />
      </h1>
      {text && <p className={styles.text}>{text}</p>}
      <div className={styles.card} data-vt="card">
        {children}
      </div>
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
        <header className={styles.rail}>
          <span className={styles.brand}>
            <Logo size={22} />
            Echo
          </span>
          <BackToSite />
        </header>
        <div className={styles.content}>{body}</div>
        <footer className={`${styles.rail} ${styles.railFoot}`}>
          <PulseLine />
          <nav className={styles.legal} aria-label="Legal">
            <Link to="/security">Security</Link>
            <Link to="/terms">Terms</Link>
          </nav>
        </footer>
      </section>
      <aside className={styles.pane}>
        <span className={styles.shine} aria-hidden="true" />
        {links}
      </aside>
    </div>
  );
}
