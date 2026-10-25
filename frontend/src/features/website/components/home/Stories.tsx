import { Link } from "react-router-dom";
import { Reveal } from "@/ui";
import { Section } from "../chrome/Section";
import { STORIES } from "../../content/home";
import { StoryCard } from "./StoryCard";
import styles from "./Stories.module.css";

export function Stories() {
  return (
    <Section id="how" top={140}>
      <div className={styles.list}>
        {STORIES.map((story, i) => (
          <Reveal key={story.number}>
            <article className={`${styles.story} ${i % 2 ? styles.flip : ""}`}>
              <div className={styles.words}>
                <p className={styles.label}>
                  <span>{story.number}</span> {story.label}
                </p>
                <h2 className={styles.title}>{story.title}</h2>
                <p className={styles.text}>{story.text}</p>
                <ul className={styles.points}>
                  {story.points.map((point) => (
                    <li key={point}>{point}</li>
                  ))}
                </ul>
                <Link to="/features" className={styles.more}>
                  Learn more
                </Link>
              </div>
              <StoryCard number={story.number} />
            </article>
          </Reveal>
        ))}
      </div>
    </Section>
  );
}
