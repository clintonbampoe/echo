import { FilterBar, FilterSelect } from "@/ui";
import {
  countActive,
  statusOptions,
  teamFilters,
  type TeamFilters,
} from "../model/filters";

type Props = {
  filters: TeamFilters;
  onChange: (change: Partial<TeamFilters>) => void;
  onClear: () => void;
};

// The team's filter: the shared bar, which is a Filters button on a tablet or phone (as on Members).
export function TeamFilterBar({ filters, onChange, onClear }: Props) {
  return (
    <FilterBar
      active={countActive(filters)}
      filtered={teamFilters.isFiltered(filters)}
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
