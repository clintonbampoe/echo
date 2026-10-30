import { api } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";

// What one category holds. Liquidated assets are not counted.
export type AssetCategoryRow = {
  id: number;
  name: string;
  count: number;
  value: number;
};

// Every category that holds assets, with how many and what they are worth now: one call for the whole card.
export const getAssetCategoryRows = (signal?: AbortSignal) =>
  api
    .get<
      {
        categoryId: number;
        categoryName: string;
        count: number;
        totalCurrentValue: number;
      }[]
    >("/v1/assets/summary/by-category", {
      query: { from: ALL_TIME.from, to: ALL_TIME.to },
      signal,
    })
    .then((rows) =>
      rows.map((r): AssetCategoryRow => ({
        id: r.categoryId,
        name: r.categoryName,
        count: r.count,
        value: r.totalCurrentValue,
      })),
    );
