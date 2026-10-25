import { FilterBar, FilterSelect, type Option } from "@/ui";
import {
  categoryOptions,
  countActive,
  periodOptions,
  projectFilters,
  statusOptions,
  type ListFilters,
} from "../../model/filters";
import type { ProjectCategory } from "../../api/categories";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
  categories: ProjectCategory[] | undefined;
  managers: readonly Option[];
};

// The Projects filters: search by name, then status, category and manager, with the years at the far right. On a
// phone or tablet everything but the search folds behind the Filters button (design rule S7).
export function ProjectFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
  categories,
  managers,
}: Props) {
  return (
    <FilterBar
      searchLabel="Search projects"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={projectFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="This year"
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
      <FilterSelect
        label="Manager"
        options={managers}
        value={filters.manager}
        onChange={(manager) => onChange({ manager })}
      />
    </FilterBar>
  );
}
