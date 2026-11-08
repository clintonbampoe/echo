import { useState } from "react";
import { Cta } from "../chrome/Cta";
import { Section } from "../chrome/Section";
import styles from "./GiftStrip.module.css";

const WAYS = ["One time", "Monthly"] as const;

// An open invitation to give, near the end of the page. Gifts are arranged by writing to us for now, so the
// switch is only the shape of the choice; it sends nothing.
type Props = { title?: string; text?: string };

export function GiftStrip({
  title = "Free because people chose to make it so.",
  text = "Echo is built for churches that should not have to choose between good software and everything else they carry. If it has helped yours, you can help it keep going. Gifts go into the servers and the time behind each improvement.",
}: Props) {
  const [way, setWay] = useState<(typeof WAYS)[number]>("One time");
  return (
    <Section top={130}>
      <div className={styles.card}>
        <div className={styles.words}>
          <h2 className={styles.title}>{title}</h2>
          <p className={styles.text}>{text}</p>
        </div>
        <div className={styles.give}>
          <div
            className={styles.switch}
            role="radiogroup"
            aria-label="How often"
          >
            {WAYS.map((label) => (
              <button
                key={label}
                type="button"
                role="radio"
                aria-checked={way === label}
                className={styles.option}
                onClick={() => setWay(label)}
              >
                {label}
              </button>
            ))}
          </div>
          <Cta to="/contact?topic=support" block className={styles.action}>
            Give what feels right
          </Cta>
        </div>
      </div>
    </Section>
  );
}
