import { ClosingCta } from "../components/chrome/ClosingCta";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { TextLink } from "../components/chrome/TextLink";
import { ROADMAP } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./RoadmapPage.module.css";

export function RoadmapPage() {
  return (
    <>
      <PageHero eyebrow="Roadmap" title={["Where Echo", "is going."]}>
        Shaped by the people who use it. Add an idea on GitHub and it joins the
        list.
      </PageHero>
      <Section top={96}>
        <div className={styles.columns}>
          {ROADMAP.map((stage) => (
            <section key={stage.when} className={styles.stage}>
              <h2 className={styles.when}>{stage.when}</h2>
              <p className={styles.note}>{stage.note}</p>
              <ul className={styles.items}>
                {stage.items.map((item) => (
                  <li key={item} className={styles.item}>
                    {item}
                  </li>
                ))}
              </ul>
            </section>
          ))}
        </div>
        <p className={styles.suggest}>
          Missing something your church needs?{" "}
          <TextLink to={`${GITHUB_URL}/issues/new`} external>
            Suggest a feature on GitHub
          </TextLink>
        </p>
      </Section>
      <ClosingCta />
    </>
  );
}
