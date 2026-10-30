import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { paths } from "@/lib/paths";
import {
  DashCard,
  DistributionBar,
  Figure,
  MonthBars,
  SegmentedField,
} from "@/ui";
import { useCategories, useMoneyMonths } from "../../hooks/money";
import {
  monthFigures,
  moneyRange,
  monthSlots,
  readMonths,
  slicesOf,
  type MoneyKind,
} from "../../model/money";
import { monthRanges, type MonthRange } from "../../model/ranges";
import { CardFailed } from "./CardFailed";
import { RangeTabs } from "../controls/RangeTabs";
import { SummaryButton } from "./WeeklySummary";
import styles from "./MoneyCard.module.css";

const kinds = [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
] as const;

type Figures = ReturnType<typeof monthFigures>;
type Category = NonNullable<ReturnType<typeof useCategories>["categories"]>;

// A bar for each month, with the month picked (the latest to begin with) giving the figure: what came in or went
// out that month, its net, and the total over the range. Keyed by range, so changing the range forgets the pick.
function MonthsPart({
  figures,
  kind,
  range,
  loaded,
}: {
  figures: Figures;
  kind: MoneyKind;
  range: MonthRange;
  loaded: boolean;
}) {
  const [picked, setPicked] = useState<string | null>(null);
  const word = kind === "Income" ? "Income" : "Expenses";
  const { of, month, value, net, total } = readMonths(figures, kind, picked);
  const shown = (n: number) => (loaded ? formatNumber(n) : "–");
  return (
    <div className={styles.chart}>
      <Figure
        label={`${word} · ${month?.slot.name ?? ""}`}
        value={shown(value)}
        unit="GHS"
        sides={[
          {
            label: "Net",
            value: `${loaded && net < 0 ? "−" : ""}${shown(Math.abs(net))}`,
          },
          { label: `Last ${range} months`, value: shown(total) },
        ]}
      />
      <MonthBars
        label={`${word} for each of the last ${range} months`}
        selected={month?.slot.key}
        onPick={(key) => setPicked((was) => (was === key ? null : key))}
        bars={figures.map((f) => ({
          key: f.slot.key,
          label: f.slot.label,
          name: f.slot.name,
          tip: f.slot.tip,
          value: of(f),
          text: formatNumber(of(f)),
        }))}
      />
    </div>
  );
}

// Where the money came from (or went to), biggest first. A category opens the finance list for it; "Other" stands
// for several, so it opens nothing.
function WherePart({
  kind,
  categories,
}: {
  kind: MoneyKind;
  categories: Category;
}) {
  const income = kind === "Income";
  const { slices } = slicesOf(categories, kind);
  return (
    <div className={styles.where}>
      <p className={styles.heading}>
        {income ? "Where it came from" : "Where it went"}
      </p>
      <DistributionBar
        label={income ? "Income by category" : "Expenses by category"}
        empty="Nothing recorded yet"
        parts={slices.map((s) => ({
          key: s.key,
          label: s.name,
          value: s.total,
          text: formatNumber(s.total),
          share: `${s.percent}%`,
          ...(s.key === "other"
            ? { closed: true }
            : {
                to: `${paths.finance}?type=${kind}&category=${s.key}&window=all`,
              }),
        }))}
      />
    </div>
  );
}

// Income or expenses over the last six or twelve months: the months on the left, where it came from on the right.
export function MoneyCard({ today }: { today: string }) {
  const [range, setRange] = useState<MonthRange>("6");
  const [kind, setKind] = useState<MoneyKind>("Income");
  const months = Number(range);
  const { from, to } = moneyRange(today, months);
  const money = useMoneyMonths(months, to);
  const categories = useCategories(from, to);
  const figures = monthFigures(monthSlots(today, months), money.months);

  return (
    <DashCard
      title="Money"
      control={
        <span className={styles.controls}>
          <SummaryButton today={today} />
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
          <MonthsPart
            key={range}
            figures={figures}
            kind={kind}
            range={range}
            loaded={Boolean(money.months)}
          />
          <WherePart kind={kind} categories={categories.categories ?? []} />
        </div>
      )}
    </DashCard>
  );
}
