import { useMedia } from "@/ui";
import { HERO_QUESTION, HERO_STATS } from "../../content/home";
import { useCycle } from "./useCycle";
import styles from "./HeroCard.module.css";

type Stat = (typeof HERO_STATS)[number];

function Tile({ stat }: { stat: Stat }) {
  return (
    <span className={styles.tile}>
      <span className={styles.tileLabel}>{stat.label}</span>
      <span className={styles.tileValue}>{stat.value}</span>
    </span>
  );
}

// On a phone the three figures share one tile that a tap turns over; elsewhere they sit in a row.
function Stats() {
  const phone = useMedia("(max-width: 599px)");
  const { index, choose } = useCycle(HERO_STATS.length, 3200);
  if (!phone) {
    return (
      <div className={`${styles.stats} ${styles.row}`}>
        {HERO_STATS.map((stat) => (
          <Tile key={stat.label} stat={stat} />
        ))}
      </div>
    );
  }
  return (
    <button
      type="button"
      className={`${styles.stats} ${styles.slider}`}
      aria-label="Show the next figure"
      onClick={() => choose((index + 1) % HERO_STATS.length, 8000)}
    >
      <span
        className={styles.track}
        style={{ transform: `translateX(${-100 * index}%)` }}
      >
        {HERO_STATS.map((stat) => (
          <Tile key={stat.label} stat={stat} />
        ))}
      </span>
      <span className={styles.pips} aria-hidden="true">
        {HERO_STATS.map((stat, i) => (
          <span
            key={stat.label}
            className={styles.pip}
            data-on={i === index || undefined}
          />
        ))}
      </span>
    </button>
  );
}

// The example beside the title: a question types itself, the answer arrives, three more figures follow.
// It repeats every ten seconds and is only a picture, so it carries no controls but the figure tile.
export function HeroCard() {
  return (
    <div className={styles.card}>
      <div className={styles.label}>Example</div>
      <div className={styles.ask}>
        <span className={styles.question}>
          <span className={styles.typed}>{HERO_QUESTION}</span>
          <span className={styles.dots}>
            <i />
            <i />
            <i />
          </span>
        </span>
        <span className={styles.send} aria-hidden="true">
          ↑
        </span>
      </div>
      <div className={`${styles.reveal} ${styles.figure}`}>248</div>
      <div className={`${styles.reveal} ${styles.two} ${styles.note}`}>
        came on Sunday, 12 more than the week before
      </div>
      <div className={`${styles.reveal} ${styles.three}`}>
        <Stats />
      </div>
    </div>
  );
}
