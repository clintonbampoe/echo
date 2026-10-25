import { FilterBar as Bar, FilterSelect } from "@/ui";
import { windowOptions } from "@/lib/windows";
import {
  countActive,
  organizationFilters,
  type ListFilters,
} from "../../model/filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
};

// The organizations list's search and time range: the shared bar. The time range is this list's only filter,
// so it sits at the far right on a wide screen and behind the "Filters" button on a phone or tablet.
export function OrganizationFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
}: Props) {
  return (
    <Bar
      searchLabel="Search organizations"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={organizationFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="All time"
          options={windowOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      {null}
    </Bar>
  );
}
