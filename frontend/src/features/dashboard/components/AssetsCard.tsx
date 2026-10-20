import { useState, type ReactNode } from "react";
import { Pager, ProgressBar, SegmentedField } from "@/ui";
import {
  amountText,
  assetHint,
  assetView,
  rowsBy,
  type AssetView,
  type By,
} from "../assets";
import { useAssets } from "../hooks";
import { shareOf, type Slice } from "../shares";
import { Breakdown } from "./Breakdown";
import { CardFailed } from "./CardFailed";
import { DashCard } from "./DashCard";
import styles from "./AssetsCard.module.css";

const options = [
  { value: "value", label: "Value" },
  { value: "count", label: "Count" },
] as const;

type Props = {
  // On a phone the value and the categories are two cards, as the phone board draws them.
  split?: boolean;
};

// The value against what it cost, and what was lost.
function ValueBlock({ view }: { view: AssetView }) {
  return (
    <div className={styles.value}>
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
  );
}

// The ring of categories, measured by value or by count.
function CategoryBlock({
  by,
  ready,
  total,
  slices,
}: {
  by: By;
  ready: boolean;
  total: number;
  slices: readonly Slice[];
}) {
  const name = by === "value" ? "Total value" : "Total assets";
  return (
    <Breakdown
      label={`Assets by category, by ${by}`}
      total={ready ? amountText(total, by) : "–"}
      totalLabel={name}
      note={name}
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
  );
}

type SplitProps = {
  value: ReactNode;
  categories: ReactNode;
  control: ReactNode;
  hint: string;
  failed: boolean;
  retry: () => void;
};

// The phone's two cards: what the church owns, and where it sits. A failure takes the whole row, so the
// reader is not asked to retry the same request twice.
function SplitAssets({
  value,
  categories,
  control,
  hint,
  failed,
  retry,
}: SplitProps) {
  if (failed)
    return (
      <DashCard title="Assets" to="/assets">
        <CardFailed onRetry={retry} />
      </DashCard>
    );
  return (
    <Pager label="Assets" titles={["Assets", "Assets by category"]}>
      <DashCard title="Assets" hint={hint} to="/assets">
        <div className={`${styles.content} ${styles.one} ${styles.solo}`}>
          {value}
        </div>
      </DashCard>
      <DashCard title="Assets by category" to="/assets" control={control}>
        <div className={`${styles.content} ${styles.one}`}>
          <div className={styles.split}>{categories}</div>
        </div>
      </DashCard>
    </Pager>
  );
}

// What the church owns: its value against what it cost, and which categories hold it. Liquidated assets are
// not counted. The value is typed in by hand, Echo does not depreciate it.
export function AssetsCard({ split = false }: Props) {
  const [by, setBy] = useState<By>("value");
  const data = useAssets();
  const view = assetView(data.figures);
  const { total, slices } = shareOf(rowsBy(data.rows ?? [], by));
  const ready = data.rows !== undefined;

  const control = (
    <SegmentedField
      joined
      hideLabel
      label="Show by category"
      options={options}
      value={by}
      onChange={setBy}
    />
  );
  const value = <ValueBlock view={view} />;
  const categories = (
    <CategoryBlock by={by} ready={ready} total={total} slices={slices} />
  );

  if (split)
    return (
      <SplitAssets
        value={value}
        categories={categories}
        control={control}
        hint={assetHint(data.figures?.count)}
        failed={data.failed}
        retry={data.retry}
      />
    );

  return (
    <DashCard
      title="Assets"
      hint={assetHint(data.figures?.count)}
      to="/assets"
      foot={{
        note: "Value is typed in by hand, Echo does not depreciate it",
        to: "/assets",
        label: "See assets",
      }}
      control={control}
    >
      {data.failed ? (
        <CardFailed onRetry={data.retry} />
      ) : (
        <div className={styles.content}>
          {value}
          <div className={styles.split}>{categories}</div>
        </div>
      )}
    </DashCard>
  );
}
