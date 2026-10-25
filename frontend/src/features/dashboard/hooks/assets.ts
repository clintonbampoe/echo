import { useQueries } from "@tanstack/react-query";
import { getAssetCategories, getAssetFigures } from "../api/assets";
import { useOnce } from "./internals";

// The assets as a whole, and each category's share. The server has no breakdown by category, so this asks
// once for the totals, once for the categories and once for each category that holds assets. `rows` is there
// only when every one has answered.
export function useAssets() {
  const overall = useOnce("asset-figures", (signal) =>
    getAssetFigures(undefined, signal),
  );
  const categories = useOnce("asset-categories", getAssetCategories);
  const used = (categories.data ?? []).filter((c) => c.assetCount > 0);
  const each = useQueries({
    queries: used.map((c) => ({
      queryKey: ["dashboard", "asset-figures", c.id],
      queryFn: ({ signal }: { signal?: AbortSignal }) =>
        getAssetFigures(c.id, signal),
    })),
  });
  const rows = each.flatMap((r, i) => {
    const c = used[i];
    return r.data && c
      ? [
          {
            key: String(c.id),
            name: c.name,
            count: r.data.count,
            value: r.data.value,
          },
        ]
      : [];
  });
  const ready =
    overall.data !== undefined &&
    categories.data !== undefined &&
    rows.length === used.length;
  return {
    figures: overall.data,
    rows: ready ? rows : undefined,
    failed: overall.failed || categories.failed || each.some((r) => r.isError),
    retry: () => {
      overall.retry();
      categories.retry();
      each.forEach((r) => {
        if (r.isError) void r.refetch();
      });
    },
  };
}
