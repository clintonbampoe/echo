import { ClosingCta } from "../components/chrome/ClosingCta";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { CHANGELOG } from "../content/pages";
import styles from "./ChangelogPage.module.css";

export function ChangelogPage() {
  return (
    <>
      <PageHero eyebrow="Changelog" title={["What is new."]}>
        Small improvements, shipped often.
      </PageHero>
      <Section top={96}>
        <ol className={styles.list}>
          {CHANGELOG.map((entry) => (
            <li key={entry.date} className={styles.entry}>
              <time className={styles.date}>{entry.date}</time>
              <div>
                <h2 className={styles.title}>{entry.title}</h2>
                <p className={styles.text}>{entry.text}</p>
              </div>
            </li>
          ))}
        </ol>
      </Section>
      <ClosingCta />
    </>
  );
}
