import { Link } from "react-router-dom";
import { Section } from "../chrome/Section";
import styles from "./OpenStrip.module.css";

// A quiet pointer to the Community page near the bottom of the home page.
export function OpenStrip() {
  return (
    <Section top={140}>
      <div className={styles.strip}>
        <div>
          <p className={styles.label}>Community</p>
          <h2 className={styles.title}>
            Free, open source, and built with you.
          </h2>
        </div>
        <p className={styles.text}>
          Echo costs churches nothing. Read it, shape it and help it grow, or
          support the people who keep it going.
        </p>
        <Link to="/community" className={styles.link}>
          Built in the open
        </Link>
      </div>
    </Section>
  );
}
