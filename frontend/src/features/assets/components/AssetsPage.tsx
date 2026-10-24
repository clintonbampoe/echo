import { useState } from "react";
import { todayText } from "@/lib/dates";
import { paths } from "@/lib/paths";
import { useUrlFilters } from "@/lib/useUrlFilters";
import { LinkButton, PageHeader, StatStrip } from "@/ui";
import type { Asset } from "../api";
import { assetFilters, periodOptions, toApiFilters } from "../filters";
import { useAssetCategories, useAssets, useAssetSummary } from "../hooks";
import { assetStats } from "../view";
import { AssetDialogs, type AssetStart } from "./AssetDialogs";
import { AssetFilterBar } from "./AssetFilterBar";
import { AssetList } from "./AssetList";
import { AssetSheet } from "./AssetSheet";

// The Assets page: what the church owns and what it is worth, with the numbers, the filters and every asset.
export function AssetsPage() {
  const today = todayText();
  const [creating, setCreating] = useState(false);
  const [opened, setOpened] = useState<{
    asset: Asset;
    start: AssetStart;
  } | null>(null);
  const { filters, update, clear, resetKey } = useUrlFilters(assetFilters);
  const categories = useAssetCategories();
  const query = toApiFilters(filters, today);
  const list = useAssets(query);
  const summary = useAssetSummary(query);
  const period =
    periodOptions.find((o) => o.value === filters.window)?.label ?? "";

  return (
    <div className="anim-rise">
      <PageHeader
        title="Assets"
        action={
          <LinkButton to={paths.assetCategories} variant="secondary">
            Categories
          </LinkButton>
        }
        add={{ label: "New asset", onClick: () => setCreating(true) }}
      />
      <StatStrip
        label="Assets summary"
        stats={assetStats(
          summary.data,
          period,
          filters.status !== "Liquidated",
        )}
        busy={summary.isPending}
      />
      <AssetFilterBar
        filters={filters}
        onChange={update}
        resetKey={resetKey}
        onClear={clear}
        categories={categories.data}
      />
      <AssetList
        list={list}
        filtered={assetFilters.isFiltered(filters)}
        onAction={(asset, start) => setOpened({ asset, start })}
      />
      <AssetSheet open={creating} onClose={() => setCreating(false)} />
      {opened && (
        <AssetDialogs
          asset={opened.asset}
          start={opened.start}
          onClose={() => setOpened(null)}
        />
      )}
    </div>
  );
}
