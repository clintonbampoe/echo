import { FilterBar, FilterSelect } from "@/ui";
import type { AssetCategory } from "../api";
import {
  assetFilters,
  categoryOptions,
  countActive,
  periodOptions,
  statusOptions,
  type ListFilters,
} from "../filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
  categories: AssetCategory[] | undefined;
};

// The Assets filters: search by name, then status and category, with the days of purchase at the far right. On a phone
// or tablet everything but the search folds behind the Filters button (design rule S7).
export function AssetFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
  categories,
}: Props) {
  return (
    <FilterBar
      searchLabel="Search assets"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={assetFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="All time"
          options={periodOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      <FilterSelect
        label="Status"
        options={statusOptions}
        value={filters.status}
        onChange={(status) => onChange({ status })}
      />
      <FilterSelect
        label="Category"
        options={categoryOptions(categories)}
        value={filters.category}
        onChange={(category) => onChange({ category })}
      />
    </FilterBar>
  );
}
