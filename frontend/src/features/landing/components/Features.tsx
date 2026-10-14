import { Icon, Reveal } from "@/ui";
import { features } from "../content";
import { ProductTabs } from "./ProductTabs";
import { Section } from "./Section";
import styles from "./Features.module.css";

export function Features() {
  return (
    <>
      <Section id="features">
        <Reveal>
          <h2 className={styles.heading}>The whole week of church, handled.</h2>
          <p className={styles.lead}>
            One system for the people, the money and the plans, so nothing lives
            in a notebook or a group chat.
          </p>
        </Reveal>
        <Reveal delay={80}>
          <ProductTabs />
        </Reveal>
      </Section>

      <Section>
        <Reveal>
          <h2 className={`${styles.heading} ${styles.left}`}>
            And everything around it.
          </h2>
        </Reveal>
        <div className={styles.grid}>
          {features.map((f, i) => (
            <Reveal key={f.title} delay={i * 70}>
              <article className={styles.card}>
                <Icon name={f.icon} size={24} />
                <h3 className={styles.cardTitle}>{f.title}</h3>
                <p className={styles.cardText}>{f.text}</p>
              </article>
            </Reveal>
          ))}
        </div>
      </Section>
    </>
  );
}
