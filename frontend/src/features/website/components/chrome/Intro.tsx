import type { CSSProperties, ReactNode } from "react";
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
  // Something that sits beside the words (the home page's example), below them on a narrow screen.
  aside?: ReactNode;
  // The widths of the words and the aside, as CSS grid columns.
  columns?: string;
  top?: number;
  tablet?: number;
};

// The top of a page: a kicker, a very large title, one sentence. Left-aligned.
export function Intro({
  kicker,
  title,
  children,
  actions,
  aside,
  columns,
  top = 170,
  tablet,
}: Props) {
  const words = (
    <div>
      <Kicker>{kicker}</Kicker>
      <h1 className={styles.title}>
        {title}
        <Stop />
      </h1>
      {children && <p className={styles.lead}>{children}</p>}
      {actions && <div className={styles.actions}>{actions}</div>}
    </div>
  );
  return (
    <Section top={top} tablet={tablet} phone={100}>
      {aside ? (
        <div
          className={styles.beside}
          style={
            columns ? ({ "--columns": columns } as CSSProperties) : undefined
          }
        >
          {words}
          {aside}
        </div>
      ) : (
        words
      )}
    </Section>
  );
}
