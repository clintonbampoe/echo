import type { ReactNode } from "react";
import { Kicker } from "./Kicker";
import { Section } from "./Section";
import styles from "./Split.module.css";

type Props = {
  kicker: string;
  // The heading, with its full stop.
  title: string;
  children: ReactNode;
};

// A band of a page: a kicker and heading on the left, the content on the right. One column below 1000px.
export function Split({ kicker, title, children }: Props) {
  return (
    <Section top={130}>
      <div className={styles.split}>
        <div>
          <Kicker>{kicker}</Kicker>
          <h2 className={styles.title}>{title}</h2>
        </div>
        <div>{children}</div>
      </div>
    </Section>
  );
}
