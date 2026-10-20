import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { SegmentedField, useMedia } from "@/ui";
import { useSeries } from "../hooks";
import {
  bucketsOf,
  summarize,
  viewOf,
  views,
  wordsOf,
  type ViewKey,
} from "../trend";
import { DashCard } from "./DashCard";
import { TrendBars, type TrendBar } from "./TrendBars";
import styles from "./AttendanceCard.module.css";

const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// People present over time. One card: a wide one draws the chart beside three numbers, a narrow one puts
// the latest number above the chart. The card is the same size whatever the numbers say.
export function AttendanceCard({ today }: { today: string }) {
  const [viewKey, setViewKey] = useState<ViewKey>("weeks");
  const narrow = useMedia("(max-width: 900px)");
  const view = viewOf(viewKey);
  const words = wordsOf(view.unit);
  const buckets = bucketsOf(view, today);
  const series = useSeries(buckets);
  const summary = series.values && summarize(series.values);
  const latest = buckets[buckets.length - 1];

  const bars: TrendBar[] = buckets.map((b, i) => {
    const value = series.values?.[i];
    return {
      key: b.from,
      label: b.label,
      name: b.name,
      tip: b.tip,
      value: value ?? 0,
      text: num(value),
    };
  });

  const sides = [
    { label: words.last, value: summary?.last },
    { label: words.average, value: summary?.average },
    { label: words.best, value: summary?.best },
  ];

  return (
    <DashCard
      title="Attendance"
      hint={words.hint}
      to="/attendance"
      control={
        <SegmentedField
          joined
          hideLabel
          label="Range"
          options={views.map((v) => ({
            value: v.key,
            label: narrow ? v.short : v.label,
          }))}
          value={viewKey}
          onChange={setViewKey}
        />
      }
    >
      {series.failed ? (
        <div className={styles.failed}>
          <p>Could not load</p>
          <button type="button" className={styles.retry} onClick={series.retry}>
            Try again
          </button>
        </div>
      ) : (
        <div className={styles.content} aria-busy={!series.values}>
          <div className={styles.main}>
            <div className={styles.figure}>
              <p className={styles.big}>{num(summary?.last)}</p>
              <p className={styles.note}>
                {summary && latest
                  ? `${latest.name}, average ${formatNumber(summary.average)}`
                  : "\u00a0"}
              </p>
            </div>
            <div className={styles.chart}>
              <TrendBars
                label={`Attendance for each of the last ${view.count} ${view.unit}s`}
                bars={bars}
              />
            </div>
          </div>
          <div role="group" aria-label="Summary" className={styles.side}>
            {sides.map((s) => (
              <div key={s.label}>
                <p className={styles.label}>{s.label}</p>
                <p className={styles.value}>{num(s.value)}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </DashCard>
  );
}
