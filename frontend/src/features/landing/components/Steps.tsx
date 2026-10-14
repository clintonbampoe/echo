import { Reveal } from "@/ui";
import { steps } from "../content";
import { Section } from "./Section";
import styles from "./Steps.module.css";

export function Steps() {
  return (
    <Section id="how">
      <Reveal>
        <h2 className={styles.heading}>Up and running before Sunday.</h2>
      </Reveal>
      <ol className={styles.list}>
        {steps.map((s, i) => (
          <li key={s.title} className={styles.item}>
            <Reveal delay={i * 120}>
              <p className={styles.number}>{String(i + 1).padStart(2, "0")}</p>
              <h3 className={styles.title}>{s.title}</h3>
              <p className={styles.text}>{s.text}</p>
            </Reveal>
          </li>
        ))}
      </ol>
    </Section>
  );
}
