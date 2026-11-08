import { LinkButton, useMedia } from "@/ui";
import { Kicker } from "../chrome/Kicker";
import { Section } from "../chrome/Section";
import { ArrowLink } from "../chrome/ArrowLink";
import { WAYS } from "../../content/home";
import styles from "./TwoWays.module.css";

// Hosted by us, or on your own servers. Two cards side by side; on a phone each folds down to its name.
export function TwoWays() {
  const phone = useMedia("(max-width: 599px)");
  return (
    <Section top={130}>
      <div className={styles.head}>
        <Kicker>Two ways to run Echo</Kicker>
        <h2 className={styles.title}>Your data stays yours.</h2>
        <p className={styles.lead}>
          The same Echo either way. When privacy has to be absolute, it runs on
          your own servers.
        </p>
      </div>
      <div className={styles.cards}>
        {WAYS.map((way, i) => (
          <details
            key={way.name}
            className={styles.card}
            open={!phone || i === 0}
          >
            <summary
              className={styles.summary}
              onClick={(e) => !phone && e.preventDefault()}
            >
              <span className={styles.name}>{way.name}</span>
              <span className={styles.tag}>{way.tag}</span>
            </summary>
            <ol className={styles.points}>
              {way.points.map((point, n) => (
                <li key={point}>
                  <span>{String(n + 1).padStart(2, "0")}</span>
                  {point}
                </li>
              ))}
            </ol>
            <LinkButton
              to={way.action.to}
              size="lg"
              pill
              block
              variant={i === 0 ? "primary" : "secondary"}
            >
              {way.action.label}
            </LinkButton>
          </details>
        ))}
      </div>
      <div className={styles.links}>
        <ArrowLink to="/security">How we handle data</ArrowLink>
        <ArrowLink to="/contact?topic=private">
          See what Sanctuary includes
        </ArrowLink>
      </div>
    </Section>
  );
}
