import { useState } from "react";
import { formatMoney } from "@/lib/format";
import { useTitheSeries } from "../hooks";
import { titheView } from "../tithes";
import { bucketsOf, viewOf, views, wordsOf, type ViewKey } from "../trend";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { RangePill } from "./RangePill";
import { TrendBars, type TrendBar } from "./TrendBars";
import { TrendBody } from "./TrendBody";

const options = views.map((v) => ({ value: v.key, label: v.range }));

// Tithes for each week or month, with the latest one's figures under the chart. The footer line compares
// the latest with the one before.
export function TithesCard({ today }: { today: string }) {
  const [viewKey, setViewKey] = useState<ViewKey>("weeks");
  const view = viewOf(viewKey);
  const words = wordsOf(view.unit);
  const buckets = bucketsOf(view, today);
  const series = useTitheSeries(buckets);
  const shown = titheView(series.values, view.unit);

  const bars: TrendBar[] = buckets.map((b, i) => {
    const total = series.values?.[i]?.total;
    return {
      key: b.from,
      label: b.label,
      name: b.name,
      tip: b.tip,
      value: total ?? 0,
      text: total === undefined ? "–" : formatMoney(total),
    };
  });

  return (
    <DashCard
      tall
      title="Tithes"
      hint={`Per ${view.unit}`}
      to="/tithes"
      control={
        <RangePill
          label="Range"
          value={viewKey}
          options={options}
          onChange={setViewKey}
        />
      }
    >
      {series.failed ? (
        <CardFailed onRetry={series.retry} />
      ) : (
        <TrendBody
          busy={!series.values}
          chart={
            <TrendBars
              label={`Tithes for each of the last ${view.count} ${view.unit}s`}
              bars={bars}
            />
          }
          figures={[
            { label: words.last, value: shown.last },
            { label: "Tithers", value: shown.tithers },
            { label: "Average tithe", value: shown.average },
          ]}
          footer={shown.footer}
        />
      )}
    </DashCard>
  );
}
