import { LinkButton } from "@/ui";
import { ClosingCta } from "../components/chrome/ClosingCta";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { DASHBOARD, FEATURES } from "../content/features";
import { GITHUB_URL } from "../content/site";
import styles from "./FeaturesPage.module.css";

export function FeaturesPage() {
  return (
    <>
      <PageHero
        eyebrow="Features"
        title={["Everything a church", "runs on."]}
        actions={
          <>
            <LinkButton to="/signup" size="xl" pill className={styles.primary}>
              Get started free
            </LinkButton>
            <LinkButton
              to={`${GITHUB_URL}/tree/main/docs`}
              size="xl"
              variant="secondary"
              pill
            >
              Browse the docs
            </LinkButton>
          </>
        }
      >
        Ten tools that share one idea: put your data back in your hands, and
        make it easy to read.
      </PageHero>
      <Section>
        <ul className={styles.list}>
          {FEATURES.map((feature) => (
            <li key={feature.name} className={styles.feature}>
              <div>
                <p className={styles.name}>{feature.name}</p>
                <h2 className={styles.title}>{feature.title}</h2>
              </div>
              <ul className={styles.points}>
                {feature.points.map((point) => (
                  <li key={point}>{point}</li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      </Section>
      <Section top={140}>
        <div className={styles.dashboard}>
          <p className={styles.name}>The dashboard</p>
          <h2 className={styles.big}>{DASHBOARD.title}</h2>
          <p className={styles.text}>{DASHBOARD.text}</p>
          <ul className={styles.points}>
            {DASHBOARD.points.map((point) => (
              <li key={point}>{point}</li>
            ))}
          </ul>
        </div>
      </Section>
      <ClosingCta />
    </>
  );
}
