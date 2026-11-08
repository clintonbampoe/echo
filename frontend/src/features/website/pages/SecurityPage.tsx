import { DataArt } from "../components/chrome/DataArt";
import { Kicker } from "../components/chrome/Kicker";
import { RuleList, type Rule } from "../components/chrome/RuleList";
import { Section } from "../components/chrome/Section";
import { Stop } from "../components/chrome/Stop";
import { SECURITY } from "../content/trust";
import styles from "./LegalPage.module.css";

// The same shape as Terms: the title, one line and a picture on the left, numbered rules on the right.
export function SecurityPage() {
  return (
    <Section top={150} tablet={130} phone={100}>
      <div className={styles.layout}>
        <div>
          <Kicker>Security and privacy</Kicker>
          <h1 className={styles.title}>
            {SECURITY.title}
            <Stop />
          </h1>
          <p className={styles.lead}>{SECURITY.lead}</p>
          <DataArt />
        </div>
        <div>
          <RuleList rules={SECURITY.rules as readonly Rule[]} openFirst />
        </div>
      </div>
    </Section>
  );
}
