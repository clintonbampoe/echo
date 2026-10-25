import { api } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";

// What assets are grouped under, with how many each holds.
export type AssetCategory = { id: number; name: string; assetCount: number };

export const getAssetCategories = (signal?: AbortSignal) =>
  api.get<AssetCategory[]>("/v1/asset-categories", { signal });

// The assets' numbers, for all of them or one category. Liquidated assets are not counted. `lost` is cost
// minus value, below zero when they are worth more than they cost.
export type AssetFigures = {
  count: number;
  value: number;
  cost: number;
  lost: number;
};

export const getAssetFigures = (
  categoryId: number | undefined,
  signal?: AbortSignal,
) =>
  api
    .get<{
      totalAssets: number;
      totalCurrentValue: number;
      totalPurchaseCost: number;
      totalDepreciation: number;
    }>("/v1/assets/summary", {
      query: { from: ALL_TIME.from, to: ALL_TIME.to, categoryId },
      signal,
    })
    .then((r): AssetFigures => ({
      count: r.totalAssets,
      value: r.totalCurrentValue,
      cost: r.totalPurchaseCost,
      lost: r.totalDepreciation,
    }));
