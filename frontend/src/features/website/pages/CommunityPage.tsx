import { Cta } from "../components/chrome/Cta";
import { Kicker } from "../components/chrome/Kicker";
import { LinkRows } from "../components/chrome/LinkRows";
import { Section } from "../components/chrome/Section";
import { Steps } from "../components/chrome/Steps";
import { Stop } from "../components/chrome/Stop";
import { Togetherness } from "../components/chrome/Togetherness";
import { COMMUNITY, COMMUNITY_ROWS } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./CommunityPage.module.css";

// Two full-height screens: the welcome with the ring of people, then how a change reaches every church.
export function CommunityPage() {
  return (
    <>
      <Section top={84} tablet={90} phone={96}>
        <div className={styles.hero}>
          <div>
            <Kicker>Community</Kicker>
            <h1 className={styles.title}>
              Echo belongs to the churches that use it
              <Stop />
            </h1>
            <p className={styles.lead}>
              Open source, built in the open, shaped by people who run churches.
            </p>
            <div className={styles.actions}>
              <Cta
                to={GITHUB_URL}
                className={`${styles.btn} ${styles.primary}`}
              >
                Read the code
              </Cta>
              <Cta
                to="/contact"
                variant="secondary"
                className={`${styles.btn} ${styles.second}`}
              >
                Tell us what you need
              </Cta>
            </div>
          </div>
          <div className={styles.art}>
            <Togetherness />
          </div>
        </div>
      </Section>
      <Section top={104} tablet={104} phone={60} className={styles.more}>
        <p className={styles.label}>Be part of it</p>
        <h2 className={styles.big}>From your idea to every church.</h2>
        <div className={styles.steps}>
          <Steps items={COMMUNITY.steps} />
        </div>
        <LinkRows rows={COMMUNITY_ROWS} large swipe />
      </Section>
    </>
  );
}
