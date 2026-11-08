import type { CSSProperties, ReactNode } from "react";
import { Kicker } from "./Kicker";
import { Section } from "./Section";
import styles from "./Split.module.css";

type Props = {
  kicker: string;
  // The heading, with its full stop.
  title: string;
  // One sentence under the heading, and anything that goes after it (a link).
  lead?: string;
  after?: ReactNode;
  // Buttons under the sentence.
  actions?: ReactNode;
  // Where the two sides line up, and the space between them in pixels.
  align?: "start" | "center";
  gap?: number;
  // The space between the two parts once they stack, on a tablet and on a phone (40 and 22 unless given).
  stack?: { tablet?: number; phone?: number };
  children: ReactNode;
};

// A band of a page: a kicker and heading on the left, the content on the right. One column below 1000px.
export function Split({
  kicker,
  title,
  lead,
  after,
  actions,
  align = "start",
  gap = 100,
  stack,
  children,
}: Props) {
  return (
    <Section top={130}>
      <div
        className={styles.split}
        style={
          {
            "--gap": `${gap}px`,
            ...(stack?.tablet !== undefined && {
              "--stack-tablet": `${stack.tablet}px`,
            }),
            ...(stack?.phone !== undefined && {
              "--stack-phone": `${stack.phone}px`,
            }),
            alignItems: align,
          } as CSSProperties
        }
      >
        <div>
          <Kicker>{kicker}</Kicker>
          <h2 className={styles.title}>{title}</h2>
          {lead && <p className={styles.lead}>{lead}</p>}
          {after && <div className={styles.after}>{after}</div>}
          {actions && <div className={styles.actions}>{actions}</div>}
        </div>
        <div>{children}</div>
      </div>
    </Section>
  );
}
