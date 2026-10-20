import { useState } from "react";
import { formatNumber } from "@/lib/format";
import { useSeries } from "../hooks";
import {
  bucketsOf,
  pickedStretch,
  stretchName,
  summarize,
  viewChoices,
  viewOf,
  wordsOf,
  type ViewKey,
} from "../trend";
import { DashCard } from "./DashCard";
import { CardFailed } from "./CardFailed";
import { RangeTabs } from "./RangeTabs";
import { TrendBars, type TrendBar } from "./TrendBars";
import styles from "./AttendanceCard.module.css";

const num = (n: number | undefined) =>
  n === undefined ? "–" : formatNumber(n);

// People present over time. One card: a wide one draws the chart beside three numbers, a narrow one puts
// the number picked above the chart. Picking another bar tells the card which stretch to speak about.
export function AttendanceCard({ today }: { today: string }) {
  const [viewKey, setViewKey] = useState<ViewKey>("weeks");
  const [picked, setPicked] = useState<string | null>(null);
  const view = viewOf(viewKey);
  const words = wordsOf(view.unit);
  const buckets = bucketsOf(view, today);
  const series = useSeries(buckets);
  const summary = series.values && summarize(series.values);
  const at = pickedStretch(buckets, picked);
  const name = stretchName(at, words.last);
  const note = at.stretch
    ? `${at.stretch.name}. Tap a bar for another ${view.unit}`
    : undefined;

  const bars: TrendBar[] = buckets.map((b, i) => {
    const present = series.values?.[i];
    return {
      key: b.from,
      label: b.label,
      name: b.name,
      tip: b.tip,
      value: present ?? 0,
      text: num(present),
    };
  });

  const sides = [
    { label: name, value: series.values?.[at.index] },
    { label: words.average, value: summary?.average },
    { label: words.best, value: summary?.best },
  ];

  // A picked bar is picked again to go back to the latest stretch.
  const pickBar = (key: string) =>
    setPicked((was) => (was === key ? null : key));

  return (
    <DashCard
      title="Attendance"
      hint={words.hint}
      to="/attendance"
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
        <div className={styles.content} aria-busy={!series.values}>
          <div className={styles.main}>
            <div className={styles.figure}>
              <p className={styles.big}>{num(sides[0]?.value)}</p>
              <p className={styles.note}>{note ?? "\u00a0"}</p>
            </div>
            <div className={styles.chart}>
              <TrendBars
                label={`Attendance for each of the last ${view.count} ${view.unit}s`}
                bars={bars}
                selected={picked ?? undefined}
                onPick={pickBar}
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
