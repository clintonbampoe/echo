import { ClosingCta } from "../components/chrome/ClosingCta";
import { Columns } from "../components/chrome/Columns";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { ABOUT } from "../content/pages";
import styles from "./AboutPage.module.css";

export function AboutPage() {
  return (
    <>
      <PageHero eyebrow="About" title={["Quiet software for", "loud communities."]} />
      <Section top={96}>
        <p className={styles.lead}>{ABOUT.lead}</p>
      </Section>
      <Section top={96}>
        <Columns items={ABOUT.values} />
      </Section>
      <Section top={80}>
        <p className={styles.small}>Made in</p>
        <p className={styles.place}>Accra, Ghana</p>
      </Section>
      <ClosingCta />
    </>
  );
}
