import { useMedia } from "@/ui";
import { Kicker } from "../components/chrome/Kicker";
import { RuleList, type Rule } from "../components/chrome/RuleList";
import { Section } from "../components/chrome/Section";
import { Stop } from "../components/chrome/Stop";
import { LEGAL } from "../content/trust";
import styles from "./LegalPage.module.css";

type Props = { document: "privacy" | "terms" };

// Privacy and Terms share this shape: a title and one line on the left, numbered rules on the right.
export function LegalPage({ document }: Props) {
  const { kicker, title, rules } = LEGAL[document];
  // The first rule starts open on a tablet and wider; on a phone every rule starts closed.
  const roomy = useMedia("(min-width: 600px)");
  return (
    <Section top={150} tablet={130} phone={100}>
      <div className={styles.layout}>
        <div>
          <Kicker>{kicker}</Kicker>
          <h1 className={styles.title}>
            {title}
            <Stop />
          </h1>
          <p className={styles.lead}>{LEGAL.lead}</p>
          <p className={styles.updated}>{LEGAL.updated}</p>
        </div>
        <RuleList rules={rules as readonly Rule[]} openFirst={roomy} />
      </div>
    </Section>
  );
}
