import { FilterSelect, SearchBox } from "@/ui";
import { windowOptions } from "@/lib/windows";
import type { ListFilters } from "./filters";
import styles from "./FilterBar.module.css";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
};

// Search and the time range, side by side at every width. The shared filter bar folds its pills behind a
// "Filters" button on a phone; with only one pill that would hide it for nothing.
export function FilterBar({ filters, onChange, resetKey }: Props) {
  return (
    <div className={styles.bar}>
      <SearchBox
        key={resetKey}
        label="Search organizations"
        value={filters.name}
        onCommit={(name) => onChange({ name })}
        className={styles.search}
      />
      <FilterSelect
        label="All time"
        options={windowOptions}
        value={filters.window}
        quiet
        onChange={(window) => onChange({ window })}
      />
    </div>
  );
}
