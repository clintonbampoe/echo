import { getVerse } from "@/lib/verses";
import { Quote, Reveal } from "@/ui";
import { PrimaryCta } from "./Cta";
import { Section } from "./Section";
import styles from "./Closing.module.css";

export function Closing() {
  const verse = getVerse();
  return (
    <Section top={160}>
      <div className={styles.closing}>
        <div className={styles.glow} aria-hidden="true" />
        <Reveal>
          <Quote
            text={verse.text}
            reference={verse.reference}
            size="lg"
            align="center"
          />
        </Reveal>
        <Reveal delay={120} className={styles.cta}>
          <PrimaryCta />
        </Reveal>
      </div>
    </Section>
  );
}
