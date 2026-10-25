// What the Assets pages show of an asset: the numbers, what was lost and the dates. Pure, so the tests can check it
// without a screen.
import { formatDate, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { Asset, AssetSummary } from "./api";

// What an asset or a set of assets has lost: its cost minus its current value. It can be below zero (land that is
// worth more than it cost), and then it is called a gain.
export type Change = { label: string; amount: number; note: string };

export function valueChange(cost: number, value: number): Change {
  const lost = cost - value;
  if (lost < 0)
    return {
      label: "Gained value",
      amount: -lost,
      note:
        cost > 0
          ? `${Math.round((-lost / cost) * 100)}% above cost`
          : "Cost not recorded",
    };
  if (lost === 0)
    return { label: "Lost value", amount: 0, note: "Same as cost" };
  return {
    label: "Lost value",
    amount: lost,
    note: `${Math.round((lost / cost) * 100)}% below cost`,
  };
}

// The day an asset was bought, or a dash.
export const purchased = (a: Pick<Asset, "purchaseDate">): string =>
  a.purchaseDate ? formatDate(a.purchaseDate.slice(0, 10)) : "–";

// The four numbers above the list. `period` says which days of purchase they cover ("All time"). Liquidated assets are
// left out unless the status filter is Liquidated, and the tile says so.
export function assetStats(
  summary: AssetSummary | undefined,
  period: string,
  leavesOutLiquidated: boolean,
): Stat[] {
  if (!summary)
    return ["Current value", "Purchase cost", "Lost value", "Assets"].map(
      (label, i) => ({ label, value: "–", note: i === 3 ? period : "" }),
    );
  const change = valueChange(
    summary.totalPurchaseCost,
    summary.totalCurrentValue,
  );
  return [
    {
      label: "Current value",
      value: formatNumber(summary.totalCurrentValue),
      unit: "GHS",
      note: leavesOutLiquidated
        ? "Not counting liquidated"
        : "Across these assets",
    },
    {
      label: "Purchase cost",
      value: formatNumber(summary.totalPurchaseCost),
      unit: "GHS",
      note: "Across these assets",
    },
    {
      label: change.label,
      value: formatNumber(change.amount),
      unit: "GHS",
      note:
        change.label === "Gained value"
          ? "Current value minus cost"
          : "Cost minus current value",
    },
    { label: "Assets", value: String(summary.totalAssets), note: period },
  ];
}

// The three numbers on one asset's page.
export function assetFigures(a: Asset): Stat[] {
  const change = valueChange(a.purchaseCost, a.currentValue);
  return [
    {
      label: "Current value",
      value: formatNumber(a.currentValue),
      unit: "GHS",
      note: "As last entered",
    },
    {
      label: "Purchase cost",
      value: formatNumber(a.purchaseCost),
      unit: "GHS",
      note: a.purchaseDate
        ? formatDate(a.purchaseDate.slice(0, 10))
        : "No purchase date",
    },
    {
      label: change.label,
      value: formatNumber(change.amount),
      unit: "GHS",
      note: change.note,
    },
  ];
}

// The straight-line value an asset has had at the end of each year since it was bought, over `LIFE_YEARS` years, to
// the year `thisYear`. An estimate drawn from the cost; the asset's own current value is the one typed in by hand.
export const LIFE_YEARS = 10;
export type YearValue = { year: number; value: number; drop: number };

export function valueByYear(
  a: Pick<Asset, "purchaseDate" | "purchaseCost">,
  thisYear: number,
): YearValue[] {
  if (!a.purchaseDate || a.purchaseCost <= 0) return [];
  const first = Number(a.purchaseDate.slice(0, 4));
  const yearly = a.purchaseCost / LIFE_YEARS;
  const years: YearValue[] = [];
  for (let y = first; y <= thisYear; y++) {
    const value = Math.max(0, a.purchaseCost - yearly * (y - first));
    years.push({ year: y, value, drop: y === first ? 0 : yearly });
  }
  return years;
}
