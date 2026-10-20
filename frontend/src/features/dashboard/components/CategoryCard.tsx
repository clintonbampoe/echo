import { useState } from "react";
import { formatMoney } from "@/lib/format";
import { SegmentedField } from "@/ui";
import { useCategories } from "../hooks";
import {
  moneyRange,
  monthRangeLabel,
  slicesOf,
  type MoneyKind,
} from "../money";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";

const options = [
  { value: "Income", label: "Income" },
  { value: "Expense", label: "Expenses" },
] as const;

type Props = {
  today: string;
  // How many months, the same as the card beside it.
  months: number;
};

// Where the money came from, or went.
export function CategoryCard({ today, months }: Props) {
  const [kind, setKind] = useState<MoneyKind>("Income");
  const range = moneyRange(today, months);
  const data = useCategories(range.from, range.to);
  const ready = data.categories !== undefined;
  const { total, slices } = slicesOf(data.categories ?? [], kind);
  const word = kind === "Income" ? "income" : "expenses";
  const since = monthRangeLabel(months).toLowerCase();

  return (
    <DashCard
      tall
      title="By category"
      hint={monthRangeLabel(months)}
      to="/finance"
      control={
        <SegmentedField
          joined
          hideLabel
          label="Show"
          options={options}
          value={kind}
          onChange={setKind}
        />
      }
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <Breakdown
          label={`${kind === "Income" ? "Income" : "Expenses"} by category`}
          total={ready ? formatMoney(total) : "–"}
          totalLabel={`Total ${word}`}
          note={`Total ${word}, ${since}`}
          slices={slices.map((s) => ({
            key: s.key,
            name: s.name,
            value: s.total,
            text: formatMoney(s.total),
            percent: s.percent,
            color: s.color,
          }))}
          ready={ready}
          empty="Nothing recorded yet"
        />
      )}
    </DashCard>
  );
}
