import { ClosingCta } from "../components/chrome/ClosingCta";
import { Columns } from "../components/chrome/Columns";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { TextLink } from "../components/chrome/TextLink";
import { SECURITY } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./SecurityPage.module.css";

export function SecurityPage() {
  return (
    <>
      <PageHero eyebrow="Security" title={["Trusted with", "your people."]}>
        Church records are personal. We treat them that way.
      </PageHero>
      <Section top={96}>
        <Columns items={SECURITY} count={2} />
        <p className={styles.report}>
          Found a vulnerability?{" "}
          <TextLink to={`${GITHUB_URL}/security/advisories/new`} external>
            Report it privately
          </TextLink>
        </p>
      </Section>
      <ClosingCta />
    </>
  );
}
