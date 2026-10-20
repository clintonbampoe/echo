import { useState, type ReactNode } from "react";
import { formatMoney } from "@/lib/format";
import { useMoneyMonths } from "../hooks";
import { moneyRange, monthFigures, monthSlots } from "../money";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { TrendBars, type TrendBar } from "./TrendBars";
import styles from "./IncomeExpensesCard.module.css";

const money = (n: number | undefined) =>
  n === undefined ? "–" : formatMoney(n);

type Props = {
  today: string;
  // How many months, and the pill that changes it.
  months: number;
  control: ReactNode;
};

// Income and expenses month by month (tithes are counted on their own). The two figures above the chart
// belong to the month picked, this one at first, so the numbers are on the page and not only in a tooltip.
export function IncomeExpensesCard({ today, months, control }: Props) {
  const range = moneyRange(today, months);
  const data = useMoneyMonths(range.from, range.to);
  const figures = monthFigures(monthSlots(today, months), data.months);
  const [picked, setPicked] = useState<string>();
  const chosen =
    figures.find((f) => f.slot.key === picked) ?? figures[figures.length - 1];

  const bars: TrendBar[] = figures.map((f) => ({
    key: f.slot.key,
    label: f.slot.label,
    name: f.slot.name,
    tip: f.slot.tip,
    value: f.income ?? 0,
    text: money(f.income),
    second: { value: f.expenses ?? 0, text: money(f.expenses) },
  }));

  return (
    <DashCard
      title="Income and expenses"
      hint="Apart from tithes"
      to="/finance"
      control={control}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content} aria-busy={!data.months}>
          <div
            role="group"
            aria-label="Selected month"
            className={styles.stats}
          >
            <div>
              <p className={styles.key}>
                <i className={styles.income} />
                Income
              </p>
              <p className={styles.value}>{money(chosen?.income)}</p>
            </div>
            <div>
              <p className={styles.key}>
                <i className={styles.expenses} />
                Expenses
              </p>
              <p className={styles.value}>{money(chosen?.expenses)}</p>
            </div>
          </div>
          <p className={styles.note}>
            {chosen
              ? `${chosen.slot.name}. Pick a bar for another month`
              : "\u00a0"}
          </p>
          <div className={styles.chart}>
            <TrendBars
              label={`Income and expenses for each of the last ${bars.length} months`}
              bars={bars}
              selected={chosen?.slot.key}
              onPick={setPicked}
              legend={["Income", "Expenses"]}
            />
          </div>
        </div>
      )}
    </DashCard>
  );
}
