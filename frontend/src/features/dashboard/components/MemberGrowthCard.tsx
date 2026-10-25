import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { MonthBars } from "@/ui";
import { growthBuckets } from "../growth";
import { useGrowth, useMix } from "../hooks";
import { Aside, DashCard } from "./DashCard";
import { CardFailed } from "./CardFailed";
import { Figure } from "./Figure";

const MONTHS = 6;
const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// New members each of the last six months, by the day they joined, and how many members there are in all.
export function MemberGrowthCard({ today }: { today: string }) {
  const [picked, setPicked] = useState<string | null>(null);
  const buckets = growthBuckets(today, MONTHS);
  const growth = useGrowth(buckets);
  const total = useMix().mix?.total;
  const found = buckets.findIndex((b) => b.from === picked);
  const at = found < 0 ? buckets.length - 1 : found;
  const month = buckets[at];
  const joined = growth.values?.[at];

  return (
    <DashCard title="Member growth" control={<Aside>Last 6 months</Aside>}>
      {growth.failed ? (
        <CardFailed onRetry={growth.retry} />
      ) : (
        <>
          <Figure
            label={month?.name ?? ""}
            value={joined === undefined ? "–" : `+${formatNumber(joined)}`}
            unit="new members"
            sides={[{ label: "Total", value: num(total) }]}
          />
          <MonthBars
            label={`New members for each of the last ${MONTHS} months`}
            selected={month?.from}
            onPick={(key) => setPicked((was) => (was === key ? null : key))}
            bars={buckets.map((b, i) => ({
              key: b.from,
              label: b.label,
              name: b.name,
              tip: b.tip,
              value: growth.values?.[i] ?? 0,
              text: num(growth.values?.[i]),
            }))}
          />
        </>
      )}
    </DashCard>
  );
}
