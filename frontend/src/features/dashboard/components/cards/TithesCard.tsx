import { useState } from "react";
import { formatMoney, formatNumber } from "@/lib/format";
import { DashCard, Figure, MonthBars } from "@/ui";
import { growthBuckets } from "../../model/growth";
import { useTitheSeries } from "../../hooks/series";
import { monthRanges, type MonthRange } from "../../model/ranges";
import { CardFailed } from "./CardFailed";
import { RangeTabs } from "../controls/RangeTabs";

const NONE = "–";

// Tithes month by month. The month picked (the latest to begin with) gives the figure, how many people gave
// and what the average gift was.
export function TithesCard({ today }: { today: string }) {
  const [range, setRange] = useState<MonthRange>("6");
  const [picked, setPicked] = useState<string | null>(null);
  const buckets = growthBuckets(today, Number(range));
  const series = useTitheSeries(buckets);
  const found = buckets.findIndex((b) => b.from === picked);
  const at = found < 0 ? buckets.length - 1 : found;
  const month = buckets[at];
  const now = series.values?.[at];

  return (
    <DashCard
      title="Tithes"
      control={
        <RangeTabs
          label="Range"
          options={monthRanges}
          value={range}
          onChange={(r) => {
            setRange(r);
            setPicked(null);
          }}
        />
      }
    >
      {series.failed ? (
        <CardFailed onRetry={series.retry} />
      ) : (
        <>
          <Figure
            label={month?.name ?? ""}
            value={now ? formatNumber(now.total) : NONE}
            unit="GHS"
            sides={[
              {
                label: "Tithers",
                value: now ? formatNumber(now.tithers) : NONE,
              },
              {
                label: "Average",
                value: now ? formatNumber(Math.round(now.average)) : NONE,
              },
            ]}
          />
          <MonthBars
            label={`Tithes for each of the last ${range} months`}
            selected={month?.from}
            onPick={(key) => setPicked((was) => (was === key ? null : key))}
            bars={buckets.map((b, i) => {
              const total = series.values?.[i]?.total;
              return {
                key: b.from,
                label: b.label,
                name: b.name,
                tip: b.tip,
                value: total ?? 0,
                text: total === undefined ? NONE : formatMoney(total),
              };
            })}
          />
        </>
      )}
    </DashCard>
  );
}
