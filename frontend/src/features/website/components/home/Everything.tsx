import { Reveal } from "@/ui";
import { Section } from "../chrome/Section";
import { EVERYTHING } from "../../content/home";
import styles from "./Everything.module.css";

export function Everything() {
  return (
    <Section top={140}>
      <p className={styles.eyebrow}>And everything else</p>
      <h2 className={styles.title}>The rest of the week, handled.</h2>
      <ul className={styles.grid}>
        {EVERYTHING.map((item, i) => (
          <li key={item.title}>
            <Reveal delay={i * 60} className={styles.item}>
              <h3 className={styles.name}>{item.title}</h3>
              <p className={styles.text}>{item.text}</p>
            </Reveal>
          </li>
        ))}
      </ul>
    </Section>
  );
}
