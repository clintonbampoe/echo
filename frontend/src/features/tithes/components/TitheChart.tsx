import { formatMoney } from "@/lib/format";
import { MonthBars } from "@/ui";
import { methodOf, type ListFilters } from "../filters";
import { useTitheMonths } from "../hooks";
import { chartBars, chartRange, highestMonth } from "../view";
import styles from "./TitheChart.module.css";

type Props = {
  filters: ListFilters;
  today: string;
  onPick: (month: string) => void;
};

// "Collected per month": the last twelve months, by the month each tithe is for. It follows the search and the
// method, but not the dates (it is the long view). Clicking a bar picks that month for the list below.
export function TitheChart({ filters, today, onPick }: Props) {
  const months = useTitheMonths({
    memberName: filters.name.trim() || undefined,
    paymentMethod: methodOf(filters.method),
  });
  const bars = chartBars(months.data ?? []);
  const highest = highestMonth(bars);
  const chosen = /^\d{4}-\d{2}$/.test(filters.forMonth)
    ? filters.forMonth
    : today.slice(0, 7);
  return (
    <section
      aria-label="Collected per month"
      aria-busy={months.isPending}
      className={styles.card}
    >
      <h2 className={styles.title}>Collected per month</h2>
      <p className={styles.sub}>
        <span>
          {months.isError
            ? "We could not load the months. Try again in a moment."
            : chartRange(bars)}
        </span>
        {highest && <strong className={styles.top}>{highest}</strong>}
      </p>
      {bars.length > 0 && (
        <MonthBars
          label="Collected per month"
          bars={bars.map((b) => ({ ...b, text: formatMoney(b.value) }))}
          selected={chosen}
          onPick={(key) => onPick(key === filters.forMonth ? "" : key)}
        />
      )}
    </section>
  );
}
