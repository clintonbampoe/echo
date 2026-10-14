import { Icon, Reveal } from "@/ui";
import { Section } from "./Section";
import styles from "./Trust.module.css";

export function Trust() {
  return (
    <Section>
      <Reveal>
        <div className={styles.panel}>
          <div className={styles.copy}>
            <h2 className={styles.title}>
              Your church’s data stays your church’s.
            </h2>
            <p className={styles.text}>
              Every congregation on Echo is kept separate. People only ever see
              their own church’s records, and you choose each person’s role when
              you invite them.
            </p>
          </div>
          <Icon name="shield" size={72} className={styles.icon} />
        </div>
      </Reveal>
    </Section>
  );
}
