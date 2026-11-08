import { FilterBar, FilterSelect } from "@/ui";
import {
  countActive,
  staffFilters,
  statusOptions,
  type StaffFilters,
} from "../model/filters";

type Props = {
  filters: StaffFilters;
  // Names the search box, e.g. "Search staff" or "Search admins".
  searchLabel: string;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onChange: (change: Partial<StaffFilters>) => void;
  onClear: () => void;
};

// The staff filters: a search box and the status, in the shared bar (a Filters button on a tablet or phone, as on Members).
export function StaffFilterBar({
  filters,
  searchLabel,
  resetKey,
  onChange,
  onClear,
}: Props) {
  return (
    <FilterBar
      searchLabel={searchLabel}
      search={filters.q}
      onSearch={(q) => onChange({ q })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={staffFilters.isFiltered(filters)}
      onClear={onClear}
    >
      <FilterSelect
        label="Status"
        options={statusOptions}
        value={filters.status}
        onChange={(status) => onChange({ status })}
      />
    </FilterBar>
  );
}
