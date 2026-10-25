import { useState } from "react";
import { Link } from "react-router-dom";
import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import { Icon, MonthBars, SegmentedField } from "@/ui";
import { useCategories, useMoneyMonths } from "../hooks";
import {
  monthFigures,
  moneyRange,
  monthSlots,
  slicesOf,
  type MoneyKind,
} from "../money";
import { monthRanges, type MonthRange } from "../ranges";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { Figure } from "./Figure";
import { RangeTabs } from "./RangeTabs";
import { Split } from "./Split";
import styles from "./MoneyCard.module.css";

const kinds = [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
] as const;

// Income or expenses over the last six or twelve months: the total and a bar for each month on the left, and on
// the right where the money came from (or went to), biggest first.
export function MoneyCard({ today }: { today: string }) {
  const [range, setRange] = useState<MonthRange>("12");
  const [kind, setKind] = useState<MoneyKind>("Income");
  const months = Number(range);
  const { from, to } = moneyRange(today, months);
  const money = useMoneyMonths(from, to);
  const categories = useCategories(from, to);
  const figures = monthFigures(monthSlots(today, months), money.months);
  const income = kind === "Income";
  const valueOf = (f: (typeof figures)[number]) =>
    (income ? f.income : f.expenses) ?? 0;
  const total = figures.reduce((n, f) => n + valueOf(f), 0);
  const { slices } = slicesOf(categories.categories ?? [], kind);
  const last = figures[figures.length - 1];

  return (
    <DashCard
      title="Money"
      control={
        <span className={styles.controls}>
          <Link to={paths.finance} className={styles.summary}>
            <Icon name="reports" size={14} />
            Summary
          </Link>
          <RangeTabs
            label="Range"
            options={monthRanges}
            value={range}
            onChange={setRange}
          />
        </span>
      }
    >
      <div className={styles.kind}>
        <SegmentedField
          joined
          compact
          hideLabel
          label="Kind of money"
          options={kinds}
          value={kind}
          onChange={setKind}
        />
      </div>
      {money.failed || categories.failed ? (
        <CardFailed
          onRetry={() => {
            money.retry();
            categories.retry();
          }}
        />
      ) : (
        <div className={styles.body}>
          <div className={styles.chart}>
            <Figure
              label={`${income ? "Income" : "Expenses"} · last ${range} months`}
              value={money.months ? formatNumber(total) : "–"}
              unit="GHS"
            />
            <MonthBars
              label={`${income ? "Income" : "Expenses"} for each of the last ${range} months`}
              selected={last?.slot.key}
              bars={figures.map((f) => ({
                key: f.slot.key,
                label: f.slot.label,
                name: f.slot.name,
                tip: f.slot.tip,
                value: valueOf(f),
                text: formatNumber(valueOf(f)),
              }))}
            />
          </div>
          <div className={styles.where}>
            <p className={styles.heading}>
              {income ? "Where it came from" : "Where it went"}
            </p>
            <Split
              label={income ? "Income by category" : "Expenses by category"}
              empty="Nothing recorded yet"
              parts={slices.map((s) => ({
                key: s.key,
                name: s.name,
                value: s.total,
                text: formatNumber(s.total),
                share: `${s.percent}%`,
              }))}
            />
          </div>
        </div>
      )}
    </DashCard>
  );
}
