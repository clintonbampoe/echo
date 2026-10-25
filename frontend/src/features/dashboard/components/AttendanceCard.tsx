import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { MonthBars } from "@/ui";
import { growthBuckets } from "../growth";
import { usePresenceSeries } from "../hooks";
import { monthRanges, type MonthRange } from "../ranges";
import { DashCard } from "./DashCard";
import { CardFailed } from "./CardFailed";
import { Figure } from "./Figure";
import { RangeTabs } from "./RangeTabs";

const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// People present, month by month. The month picked (the latest to begin with) is the one the figure speaks about.
export function AttendanceCard({ today }: { today: string }) {
  const [range, setRange] = useState<MonthRange>("12");
  const [picked, setPicked] = useState<string | null>(null);
  const buckets = growthBuckets(today, Number(range));
  const series = usePresenceSeries(buckets);
  const found = buckets.findIndex((b) => b.from === picked);
  const at = found < 0 ? buckets.length - 1 : found;
  const month = buckets[at];
  const now = series.values?.[at];

  return (
    <DashCard
      title="Attendance"
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
            value={num(now?.present)}
            unit="present"
            sides={[
              { label: "Members", value: num(now?.members) },
              { label: "Visitors", value: num(now?.visitors) },
            ]}
          />
          <MonthBars
            label={`Attendance for each of the last ${range} months`}
            selected={month?.from}
            onPick={(key) => setPicked((was) => (was === key ? null : key))}
            bars={buckets.map((b, i) => ({
              key: b.from,
              label: b.label,
              name: b.name,
              tip: b.tip,
              value: series.values?.[i]?.present ?? 0,
              text: num(series.values?.[i]?.present),
            }))}
          />
        </>
      )}
    </DashCard>
  );
}
