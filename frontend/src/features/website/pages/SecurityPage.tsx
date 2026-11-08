import { useMedia } from "@/ui";
import { DataArt } from "../components/chrome/DataArt";
import { Kicker } from "../components/chrome/Kicker";
import { RuleList, type Rule } from "../components/chrome/RuleList";
import { Section } from "../components/chrome/Section";
import { Stop } from "../components/chrome/Stop";
import { TextLink } from "../components/chrome/TextLink";
import { SECURITY } from "../content/trust";
import { GITHUB_URL } from "../content/site";
import styles from "./LegalPage.module.css";

// The same shape as Terms: the title, one line and a picture on the left, numbered rules on the right.
export function SecurityPage() {
  const wide = useMedia("(min-width: 1000px)");
  return (
    <Section top={72}>
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
          <RuleList
            rules={SECURITY.rules as readonly Rule[]}
            openFirst={wide}
          />
          <p className={styles.report}>
            Found a vulnerability?{" "}
            <TextLink to={`${GITHUB_URL}/security/advisories/new`} external>
              Report it privately
            </TextLink>
          </p>
        </div>
      </div>
    </Section>
  );
}
