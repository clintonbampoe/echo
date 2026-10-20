import { useState } from "react";
import { ProgressBar, SegmentedField } from "@/ui";
import { amountText, assetHint, assetView, rowsBy, type By } from "../assets";
import { useAssets } from "../hooks";
import { shareOf } from "../shares";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./AssetsCard.module.css";

const options = [
  { value: "value", label: "Value" },
  { value: "count", label: "Count" },
] as const;

// What the church owns: its value against what it cost, and which categories hold it. Liquidated assets are
// not counted. The value is typed in by hand, Echo does not depreciate it.
export function AssetsCard() {
  const [by, setBy] = useState<By>("value");
  const data = useAssets();
  const view = assetView(data.figures);
  const { total, slices } = shareOf(rowsBy(data.rows ?? [], by));
  const ready = data.rows !== undefined;

  return (
    <DashCard
      tall
      title="Assets"
      hint={assetHint(data.figures?.count)}
      to="/assets"
      control={
        <SegmentedField
          joined
          hideLabel
          label="Show by category"
          options={options}
          value={by}
          onChange={setBy}
        />
      }
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content}>
          <div>
            <p className={styles.label}>Current value</p>
            <p className={styles.big}>{view.value ?? "–"}</p>
            <div className={styles.meter}>
              <ProgressBar
                value={view.share}
                label="Current value against purchase cost"
              />
            </div>
            <p className={styles.note}>{view.note}</p>
            <div className={styles.duo}>
              <div>
                <p className={styles.label}>Purchase cost</p>
                <p className={styles.amount}>{view.cost ?? "–"}</p>
              </div>
              <div>
                <p className={styles.label}>Lost value</p>
                <p className={styles.amount}>{view.lost ?? "–"}</p>
              </div>
            </div>
          </div>
          <div className={styles.split}>
            <Breakdown
              label={`Assets by category, by ${by}`}
              total={ready ? amountText(total, by) : "–"}
              totalLabel={by === "value" ? "Total value" : "Total assets"}
              slices={slices.map((s) => ({
                key: s.key,
                name: s.name,
                value: s.total,
                text: amountText(s.total, by),
                percent: s.percent,
                color: s.color,
              }))}
              ready={ready}
              empty="No assets yet"
            />
          </div>
        </div>
      )}
    </DashCard>
  );
}
