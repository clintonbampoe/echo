import type { ReactNode } from "react";
import { Kicker } from "./Kicker";
import { Section } from "./Section";
import { Stop } from "./Stop";
import styles from "./Intro.module.css";

type Props = {
  kicker: string;
  // The title, without its full stop: the stop is drawn here.
  title: string;
  children?: string;
  // Buttons or links under the sentence.
  actions?: ReactNode;
};

// The top of a page: a kicker, a very large title, one sentence. Left-aligned.
export function Intro({ kicker, title, children, actions }: Props) {
  return (
    <Section top={170}>
      <Kicker>{kicker}</Kicker>
      <h1 className={styles.title}>
        {title}
        <Stop />
      </h1>
      {children && <p className={styles.lead}>{children}</p>}
      {actions && <div className={styles.actions}>{actions}</div>}
    </Section>
  );
}
