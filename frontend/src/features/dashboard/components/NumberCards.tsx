import { buildCards } from "../cards";
import { useNumbers } from "../hooks";
import { periodOf, spanOf, type PeriodKey } from "../periods";
import { NumberCard } from "./NumberCard";
import styles from "./NumberCards.module.css";

type Props = { period: PeriodKey; today: string };

// Members, Present, Tithes and Net income for the period picked. Each card asks for its own
// numbers, so one that fails says so by itself and the rest carry on.
export function NumberCards({ period, today }: Props) {
  const numbers = useNumbers(spanOf(period, today), today);
  const cards = buildCards(numbers, periodOf(period));
  return (
    <section aria-label="Numbers for the period" className={styles.wrap}>
      <div className={styles.grid}>
        {cards.map((c) => (
          <NumberCard key={c.key} card={c} />
        ))}
      </div>
    </section>
  );
}
