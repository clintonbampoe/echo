import { formatNumber } from "@/lib/format";
import { EmptyState, ListHeader, MonthBars, type MonthBar } from "@/ui";
import type { Asset } from "../api";
import { valueByYear } from "../view";
import styles from "./AssetValue.module.css";

// How the asset's worth falls over the years, as an estimate: its cost spread evenly over ten years. Bars by year,
// then a line per year, newest first, with what that year took off.
export function AssetValue({
  asset: a,
  thisYear,
}: {
  asset: Asset;
  thisYear: number;
}) {
  const years = valueByYear(a, thisYear);
  if (years.length === 0)
    return (
      <EmptyState compact icon="assets" title="No value history yet">
        Add the purchase date and cost to the asset to see how its value falls
        over the years.
      </EmptyState>
    );
  const bars: MonthBar[] = years.map((y) => ({
    key: String(y.year),
    label: String(y.year),
    name: String(y.year),
    tip: String(y.year),
    value: y.value,
    text: `GH₵ ${formatNumber(y.value)}`,
  }));
  return (
    <div className={styles.stack}>
      <section aria-label="Value over time" className={styles.card}>
        <h2 className={styles.title}>Value over time</h2>
        <p className={styles.sub}>Straight line, 10 years. An estimate.</p>
        <div className={styles.chart}>
          <MonthBars
            label="Value by year"
            bars={bars}
            selected={String(thisYear)}
          />
        </div>
      </section>
      <section aria-label="By year" className={styles.card}>
        <ListHeader title="By year" summary={`${years.length} years`} />
        <ul className={styles.list}>
          {[...years].reverse().map((y) => (
            <li key={y.year} className={styles.row}>
              <span className={styles.year}>{y.year}</span>
              <span className={styles.drop}>
                {y.drop > 0 ? `Down ${formatNumber(y.drop)}` : "Bought"}
              </span>
              <strong className={styles.value}>{formatNumber(y.value)}</strong>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
