import { getAssetCategoryRows } from "../api/assets";
import { useOnce } from "./internals";

// The assets as a whole, and each category's share, from one call. The whole is the categories added up.
export function useAssets() {
  const { data, failed, retry } = useOnce(
    "asset-categories",
    getAssetCategoryRows,
  );
  return {
    figures: data && {
      count: data.reduce((n, r) => n + r.count, 0),
      value: data.reduce((n, r) => n + r.value, 0),
    },
    rows: data?.map((r) => ({
      key: String(r.id),
      name: r.name,
      count: r.count,
      value: r.value,
    })),
    failed,
    retry,
  };
}
