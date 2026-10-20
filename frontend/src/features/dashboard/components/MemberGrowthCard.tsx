import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { growthBuckets, growthView } from "../growth";
import { useGrowth, useNewMembers } from "../hooks";
import {
  DEFAULT_MONTHS,
  MONTH_RANGES,
  monthChoices,
  monthRangeLabel,
  type MonthRange,
} from "../money";
import { spanOf } from "../periods";
import { pickedStretch, stretchName } from "../trend";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { RangeTabs } from "./RangeTabs";
import { TrendBars, type TrendBar } from "./TrendBars";
import { TrendBody } from "./TrendBody";

const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// New members count up, so the card leads with a plus: "+31".
const signed = (text: string | undefined) =>
  text === undefined || text === "–" ? text : `+${text}`;

// New members each month, by the day they joined. This month is the dark bar, counted as far as today.
// Picking a month tells the card to speak about that month instead of the whole range.
export function MemberGrowthCard({ today }: { today: string }) {
  const [months, setMonths] = useState<MonthRange>(DEFAULT_MONTHS);
  const [picked, setPicked] = useState<string | null>(null);
  const buckets = growthBuckets(today, months);
  const growth = useGrowth(buckets);
  const lastWeek = useNewMembers(spanOf("last-week", today));
  const shown = growthView(growth.values, lastWeek.value);
  const at = pickedStretch(buckets, picked);
  const name = stretchName(at, monthRangeLabel(months));
  const monthValue = growth.values?.[at.index];
  const value = at.chosen ? num(monthValue) : shown.total;
  const note = at.chosen
    ? `New members in ${name}`
    : `New members, last ${months} months`;

  const bars: TrendBar[] = buckets.map((b, i) => {
    const count = growth.values?.[i];
    return {
      key: b.from,
      label: b.label,
      name: b.name,
      tip: b.tip,
      value: count ?? 0,
      text: num(count),
    };
  });

  return (
    <DashCard
      title="Member growth"
      hint="New members each month"
      to="/members"
      foot={{
        note: "By the date they joined",
        to: "/members",
        label: "See members",
      }}
      control={
        <RangeTabs
          label="Range"
          options={monthChoices}
          value={String(months)}
          onChange={(next) => {
            setMonths(
              MONTH_RANGES.find((m) => String(m) === next) ?? DEFAULT_MONTHS,
            );
            setPicked(null);
          }}
        />
      }
    >
      {growth.failed ? (
        <CardFailed onRetry={growth.retry} />
      ) : (
        <TrendBody
          busy={!growth.values}
          lead={{ value: signed(value), note }}
          chart={
            <TrendBars
              soft
              ends
              label={`New members for each of the last ${months} months`}
              bars={bars}
              selected={picked ?? undefined}
              onPick={(key) => setPicked((was) => (was === key ? null : key))}
            />
          }
          figures={[
            { label: name, value },
            { label: "Last week", value: shown.lastWeek },
            { label: "Average per month", value: shown.average },
          ]}
        />
      )}
    </DashCard>
  );
}
