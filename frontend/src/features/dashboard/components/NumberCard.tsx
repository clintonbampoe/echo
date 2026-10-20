import { Chip, Icon } from "@/ui";
import type { CardView } from "../cards";
import styles from "./NumberCard.module.css";

// One number with a name, how it moved and what it is compared with. The card has the same four
// lines and the same height whatever they say, so nothing shifts when the period changes.
export function NumberCard({ card }: { card: CardView }) {
  const { chip } = card;
  return (
    <div
      role="group"
      aria-label={card.label}
      aria-busy={card.state === "loading"}
      data-key={card.key}
      className={styles.card}
    >
      <p className={styles.label}>{card.label}</p>
      {card.state === "loading" ? (
        <span className={styles.bar} aria-hidden="true" />
      ) : (
        <p className={styles.value} title={card.value}>
          {card.value}
        </p>
      )}
      <div className={styles.line}>
        {card.state === "failed" && (
          <button type="button" className={styles.retry} onClick={card.retry}>
            Try again
          </button>
        )}
        {chip && (
          <Chip tone={chip.tone}>
            {chip.arrow && (
              <Icon
                name={chip.arrow === "up" ? "arrow-up" : "arrow-down"}
                size={12}
                className={styles.arrow}
              />
            )}
            {chip.text}
          </Chip>
        )}
        <p className={styles.note} title={card.note}>
          {card.note}
        </p>
      </div>
    </div>
  );
}
