import { useState } from "react";
import styles from "./RuleList.module.css";

export type Rule = {
  title: string;
  summary?: string;
  body: readonly string[];
};

// One numbered rule: the plain line shows, the full wording opens under it. A small dot marks a rule
// once it has been opened. Nothing is remembered or sent anywhere.
function RuleRow({
  rule,
  number,
  open,
  bare,
  swipe,
}: {
  rule: Rule;
  number: number;
  open: boolean;
  bare: boolean;
  swipe: boolean;
}) {
  const [read, setRead] = useState(false);
  return (
    <details
      className={bare ? `${styles.rule} ${styles.bare}` : styles.rule}
      open={open}
      data-expand
      data-read={read || undefined}
      onToggle={(e) => e.currentTarget.open && setRead(true)}
    >
      <summary className={styles.summary}>
        <span>
          {(!bare || swipe) && (
            <span
              className={
                bare ? `${styles.number} ${styles.phoneOnly}` : styles.number
              }
            >
              {String(number).padStart(2, "0")}
            </span>
          )}
          <span className={styles.name}>{rule.title}</span>
          {rule.summary && <span className={styles.plain}>{rule.summary}</span>}
        </span>
      </summary>
      <div className={styles.full}>
        {rule.body.map((text) => (
          <p key={text}>{text}</p>
        ))}
      </div>
    </details>
  );
}

type Props = {
  rules: readonly Rule[];
  // The first rule starts open.
  openFirst?: boolean;
  // No numbers: the rows start at the edge.
  bare?: boolean;
  // On a phone the rules become cards that swipe sideways, numbered.
  swipe?: boolean;
};

export function RuleList({
  rules,
  openFirst = false,
  bare = false,
  swipe = false,
}: Props) {
  return (
    <div className={swipe ? `${styles.rules} ${styles.swipe}` : styles.rules}>
      {rules.map((rule, i) => (
        <RuleRow
          key={rule.title}
          rule={rule}
          number={i + 1}
          open={openFirst && i === 0}
          bare={bare}
          swipe={swipe}
        />
      ))}
    </div>
  );
}
