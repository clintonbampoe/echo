import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { growthBuckets, growthView } from "../growth";
import { useGrowth, useNewMembers } from "../hooks";
import {
  DEFAULT_MONTHS,
  MONTH_RANGES,
  monthRangeLabel,
  type MonthRange,
} from "../money";
import { spanOf } from "../periods";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { RangePill } from "./RangePill";
import { TrendBars, type TrendBar } from "./TrendBars";
import { TrendBody } from "./TrendBody";

const options = MONTH_RANGES.map((m) => ({
  value: String(m),
  label: monthRangeLabel(m),
}));

// New members each month, by the day they joined. This month is the dark bar, counted as far as today.
export function MemberGrowthCard({ today }: { today: string }) {
  const [months, setMonths] = useState<MonthRange>(DEFAULT_MONTHS);
  const pick = (value: string) =>
    setMonths(MONTH_RANGES.find((m) => String(m) === value) ?? DEFAULT_MONTHS);
  const buckets = growthBuckets(today, months);
  const growth = useGrowth(buckets);
  const lastWeek = useNewMembers(spanOf("last-week", today));
  const shown = growthView(growth.values, lastWeek.value);

  const bars: TrendBar[] = buckets.map((b, i) => {
    const value = growth.values?.[i];
    return {
      key: b.from,
      label: b.label,
      name: b.name,
      tip: b.tip,
      value: value ?? 0,
      text: value === undefined ? "–" : formatNumber(value),
    };
  });

  return (
    <DashCard
      tall
      title="Member growth"
      hint="New members each month"
      to="/members"
      control={
        <RangePill
          label="Range"
          value={String(months)}
          options={options}
          onChange={pick}
        />
      }
    >
      {growth.failed ? (
        <CardFailed onRetry={growth.retry} />
      ) : (
        <TrendBody
          busy={!growth.values}
          chart={
            <TrendBars
              label={`New members for each of the last ${months} months`}
              bars={bars}
            />
          }
          figures={[
            { label: monthRangeLabel(months), value: shown.total },
            { label: "Last week", value: shown.lastWeek },
            { label: "Average per month", value: shown.average },
          ]}
          footer="By the date they joined"
        />
      )}
    </DashCard>
  );
}
