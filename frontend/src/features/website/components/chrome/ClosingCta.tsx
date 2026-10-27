import { LinkButton } from "@/ui";
import { Section } from "./Section";
import { Ripples } from "./Ripples";
import styles from "./ClosingCta.module.css";

// "Ready when you are." closes every page.
export function ClosingCta() {
  return (
    <Section top={140}>
      <div className={styles.closing}>
        <Ripples className={styles.ripples} />
        <h2 className={styles.title}>
          <span>Ready when</span>
          <span>you are.</span>
        </h2>
        <div className={styles.actions}>
          <LinkButton to="/signup" size="xl" pill className={styles.primary}>
            Create your church account
          </LinkButton>
          <LinkButton to="/contact" size="xl" variant="secondary" pill>
            Talk to us
          </LinkButton>
        </div>
      </div>
    </Section>
  );
}
