import { MonthBars } from "@/ui";
import { chartRange } from "../filters";
import { useTransactionMonths } from "../hooks";
import { chartBars } from "../view";
import styles from "./FinanceChart.module.css";

// "Income and expenses per month": the last twelve months, whatever days the list below is showing.
// It is the long view, so it follows neither the period nor the filters.
export function FinanceChart({ today }: { today: string }) {
  const months = useTransactionMonths(chartRange(today));
  const rows = months.data ?? [];
  return (
    <section
      aria-label="Income and expenses per month"
      aria-busy={months.isPending}
      className={styles.card}
    >
      <header className={styles.head}>
        <h2 className={styles.title}>Income and expenses per month</h2>
        <div className={styles.details}>
          {months.isError && (
            <p className={styles.line}>
              We could not load the months. Try again in a moment.
            </p>
          )}
        </div>
        <p className={styles.legend} aria-hidden="true">
          <span className={styles.key}>
            <span className={styles.swatch} />
            Income
          </span>
          <span className={styles.key}>
            <span className={`${styles.swatch} ${styles.dim}`} />
            Expenses
          </span>
        </p>
      </header>
      <MonthBars
        label="Income and expenses per month"
        bars={chartBars(rows)}
        selected={today.slice(0, 7)}
        legend={["Income", "Expenses"]}
      />
    </section>
  );
}
