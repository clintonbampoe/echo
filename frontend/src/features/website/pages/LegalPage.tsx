import { useState } from "react";
import { useMedia } from "@/ui";
import { Section } from "../components/chrome/Section";
import { LEGAL } from "../content/pages";
import styles from "./LegalPage.module.css";

type Props = { document: "privacy" | "terms" };
type Rule = {
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
}: {
  rule: Rule;
  number: number;
  open: boolean;
}) {
  const [read, setRead] = useState(false);
  return (
    <details
      className={styles.rule}
      open={open}
      data-read={read || undefined}
      onToggle={(e) => e.currentTarget.open && setRead(true)}
    >
      <summary className={styles.summary}>
        <span>
          <span className={styles.number}>
            {String(number).padStart(2, "0")}
          </span>
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

// Privacy and Terms share this shape: a title and one line on the left, numbered rules on the right.
export function LegalPage({ document }: Props) {
  const { kicker, title, rules } = LEGAL[document];
  // Wide: the first rule starts open, as on the page design. Tablet and phone: all closed.
  const wide = useMedia("(min-width: 1000px)");
  return (
    <>
      <Section top={72}>
        <div className={styles.layout}>
          <div>
            <p className={styles.kicker}>{kicker}</p>
            <h1 className={styles.title}>
              {title}
              <span className={styles.stop}>.</span>
            </h1>
            <p className={styles.lead}>{LEGAL.lead}</p>
            <p className={styles.updated}>{LEGAL.updated}</p>
          </div>
          <div className={styles.rules}>
            {(rules as readonly Rule[]).map((rule, i) => (
              <RuleRow
                key={rule.title}
                rule={rule}
                number={i + 1}
                open={wide && i === 0}
              />
            ))}
          </div>
        </div>
      </Section>
    </>
  );
}
