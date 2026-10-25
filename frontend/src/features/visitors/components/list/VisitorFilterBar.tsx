import { FilterBar as Bar, FilterSelect } from "@/ui";
import { windowOptions } from "@/lib/windows";
import {
  countActive,
  visitorFilters,
  type ListFilters,
} from "../../model/filters";
import { statusOptions } from "../../model/options";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
};

// The visitors list's filters: the shared bar, with the pills that belong to visitors.
export function VisitorFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
}: Props) {
  return (
    <Bar
      searchLabel="Search visitors"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={visitorFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="This year"
          options={windowOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      <FilterSelect
        label="Not yet members"
        options={statusOptions}
        value={filters.status}
        onChange={(status) => onChange({ status })}
      />
    </Bar>
  );
}
