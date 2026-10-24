import { Section } from "../chrome/Section";
import { ANSWERS } from "../../content/home";
import styles from "./Answers.module.css";

export function Answers() {
  return (
    <Section>
      <p className={styles.eyebrow}>Ask your church anything</p>
      <h2 className={styles.title}>The answers are already there.</h2>
      <p className={styles.lead}>
        Every record you make comes back as something you can read at a glance.
        No exports, no formulas, no waiting for a report.
      </p>
      <dl className={styles.rows}>
        {ANSWERS.map((row) => (
          <div key={row.q} className={styles.row}>
            <dt className={styles.question}>{row.q}</dt>
            <dd className={styles.answer}>
              <strong>{row.a}</strong>
              <span>{row.note}</span>
            </dd>
          </div>
        ))}
      </dl>
    </Section>
  );
}
