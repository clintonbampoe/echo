import { useState } from "react";
import { formatMoney, formatNumber } from "@/lib/format";
import { DashCard, Figure, FilterSelect, FlowBars } from "@/ui";
import { useTransactionMonths } from "../../hooks/queries";
import { flowBars } from "../../model/display";
import styles from "./FinanceChart.module.css";

const spans = [
  { value: "12", label: "12 months" },
  { value: "6", label: "6 months" },
] as const;

type Props = {
  today: string;
  // The month the list is narrowed to, '' when it is not.
  month: string;
  onPick: (month: string) => void;
};

// "Income and expenses": the last twelve (or six) months, whatever days the list below is showing. It is the long view,
// so it follows neither the period nor the filters. Clicking a month narrows the list to it.
export function FinanceChart({ today, month, onPick }: Props) {
  const [span, setSpan] = useState("12");
  const months = useTransactionMonths(Number(span), today);
  const bars = flowBars(months.data ?? []);
  const chosen = /^\d{4}-\d{2}$/.test(month) ? month : today.slice(0, 7);
  const picked = bars.find((b) => b.key === chosen);
  const net = picked ? picked.income - picked.expenses : 0;
  return (
    <DashCard
      title="Income and expenses"
      control={
        <FilterSelect
          label="Months shown"
          options={spans}
          value={span}
          quiet
          onChange={setSpan}
        />
      }
    >
      <div aria-busy={months.isPending} className={styles.body}>
        {months.isError && (
          <p className={styles.line}>
            We could not load the months. Try again in a moment.
          </p>
        )}
        {picked && (
          <Figure
            label={`${picked.name} · net`}
            value={`${net < 0 ? "−" : "+"}${formatNumber(Math.abs(net))}`}
            unit="GHS"
            sides={[
              { label: "Income", value: formatNumber(picked.income) },
              { label: "Expenses", value: formatNumber(picked.expenses) },
            ]}
          />
        )}
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
        {bars.length > 0 && (
          <FlowBars
            label="Income and expenses per month"
            bars={bars}
            selected={chosen}
            onPick={(key) => onPick(key === month ? "" : key)}
            format={formatMoney}
          />
        )}
      </div>
    </DashCard>
  );
}
