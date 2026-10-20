import { useState } from "react";
import { formatMoney } from "@/lib/format";
import { useTitheSeries } from "../hooks";
import { titheView } from "../tithes";
import {
  bucketsOf,
  pickedStretch,
  stretchName,
  viewChoices,
  viewOf,
  wordsOf,
  type ViewKey,
} from "../trend";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import { RangeTabs } from "./RangeTabs";
import { TrendBars, type TrendBar } from "./TrendBars";
import { TrendBody } from "./TrendBody";

// Tithes for each week or month, with the figures of the stretch picked under the chart (the latest at
// first). The footer line compares that stretch with the one before it.
export function TithesCard({ today }: { today: string }) {
  const [viewKey, setViewKey] = useState<ViewKey>("weeks");
  const [picked, setPicked] = useState<string | null>(null);
  const view = viewOf(viewKey);
  const words = wordsOf(view.unit);
  const buckets = bucketsOf(view, today);
  const series = useTitheSeries(buckets);
  const at = pickedStretch(buckets, picked);
  const shown = titheView(series.values, view.unit, at.index);
  const tithers = shown.tithers ?? "–";
  const note = at.stretch
    ? `${at.stretch.name}, ${tithers} tithers`
    : undefined;

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
      title="Tithes"
      hint={`Per ${view.unit}`}
      to="/tithes"
      foot={{
        note: shown.footer || undefined,
        to: "/tithes",
        label: "See tithes",
      }}
      control={
        <RangeTabs
          label="Range"
          options={viewChoices}
          value={viewKey}
          onChange={(next) => {
            setViewKey(next);
            setPicked(null);
          }}
        />
      }
    >
      {series.failed ? (
        <CardFailed onRetry={series.retry} />
      ) : (
        <TrendBody
          busy={!series.values}
          lead={{ value: shown.total, note }}
          chart={
            <TrendBars
              soft
              ends
              label={`Tithes for each of the last ${view.count} ${view.unit}s`}
              bars={bars}
              selected={picked ?? undefined}
              onPick={(key) => setPicked((was) => (was === key ? null : key))}
            />
          }
          figures={[
            { label: stretchName(at, words.last), value: shown.total },
            { label: "Tithers", value: shown.tithers },
            { label: "Average tithe", value: shown.average },
          ]}
        />
      )}
    </DashCard>
  );
}
