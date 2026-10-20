// What the Assets card says. Pure.
import { formatMoney, formatNumber, plural } from "@/lib/format";
import type { AssetFigures } from "./api";
import type { Row } from "./shares";

export type By = "value" | "count";

export type AssetView = {
  value?: string;
  // Current value as a fraction of what it all cost, for the bar.
  share: number;
  note: string;
  cost?: string;
  lost?: string;
};

export function assetView(f: AssetFigures | undefined): AssetView {
  if (!f) return { share: 0, note: "\u00a0" };
  const share = f.cost > 0 ? f.value / f.cost : 0;
  return {
    value: formatMoney(f.value),
    share,
    note:
      f.cost > 0
        ? `${Math.round(share * 100)}% of what they cost`
        : "No purchase cost recorded",
    cost: formatMoney(f.cost),
    lost: formatMoney(f.lost),
  };
}

// The line under the title. Liquidated assets are no longer owned, so they are not counted.
export const assetHint = (count?: number) =>
  count === undefined
    ? "Not counting liquidated"
    : `${formatNumber(count)} ${plural(count, "asset")}, not counting liquidated`;

// One row per category, measured by value or by how many assets it holds.
export type CategoryRow = {
  key: string;
  name: string;
  count: number;
  value: number;
};

export const rowsBy = (rows: readonly CategoryRow[], by: By): Row[] =>
  rows.map((r) => ({
    key: r.key,
    name: r.name,
    total: by === "value" ? r.value : r.count,
  }));

export const amountText = (n: number, by: By) =>
  by === "value" ? formatMoney(n) : formatNumber(n);
