import { formatMoney, formatNumber } from "@/lib/format";
import { MonthBars } from "@/ui";
import { methodOf, type ListFilters } from "../../model/filters";
import { useTitheMonths } from "../../hooks/queries";
import { chartBars, chartSummary } from "../../model/display";
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
  const summary = chartSummary(bars);
  const chosen = /^\d{4}-\d{2}$/.test(filters.forMonth)
    ? filters.forMonth
    : today.slice(0, 7);
  const picked = bars.find((b) => b.key === chosen);
  return (
    <section
      aria-label="Collected per month"
      aria-busy={months.isPending}
      className={styles.card}
    >
      <header className={styles.head}>
        <h2 className={styles.title}>Collected</h2>
        <span className={styles.range}>12 months</span>
      </header>
      {months.isError && (
        <p className={styles.line}>
          We could not load the months. Try again in a moment.
        </p>
      )}
      {picked && (
        <div className={styles.figure}>
          <p className={styles.month}>{picked.name}</p>
          <p className={styles.big}>
            {formatNumber(picked.value)}
            <span className={styles.unit}>GHS</span>
          </p>
          {summary?.highest && (
            <p className={styles.line}>
              Highest: <strong>{summary.highest.amount}</strong> in{" "}
              <strong>{summary.highest.month}</strong>
            </p>
          )}
        </div>
      )}
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
