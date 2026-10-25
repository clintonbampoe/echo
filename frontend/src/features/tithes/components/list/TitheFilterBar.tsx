import { FilterBar, FilterSelect, MonthYearPill } from "@/ui";
import {
  chooseForMonth,
  collectedOptions,
  countActive,
  methodOptions,
  titheFilters,
  type ListFilters,
} from "../../model/filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
  today: string;
};

// The Tithes filters: the shared bar, then who gave, the month it is for and how, with the days it was collected
// at the far right.
export function TitheFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
  today,
}: Props) {
  return (
    <FilterBar
      searchLabel="Search members"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={titheFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="Collected this week"
          options={collectedOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      <MonthYearPill
        label="Tithe for"
        value={filters.forMonth}
        today={today}
        onChange={(forMonth) => onChange(chooseForMonth(filters, forMonth))}
      />
      <FilterSelect
        label="Method"
        options={methodOptions}
        value={filters.method}
        onChange={(method) => onChange({ method })}
      />
    </FilterBar>
  );
}
