import { FilterBar, FilterSelect, SegmentedField } from "@/ui";
import type { Category } from "../api";
import {
  categoryOptions,
  chooseType,
  countActive,
  financeFilters,
  periodOptions,
  typeOptions,
  type ListFilters,
} from "../filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  onClear: () => void;
  categories: Category[] | undefined;
};

// The Finance filters. All / Income / Expenses is a mode switch, so it stays in view at every width and takes
// the search box's place (the server cannot search transactions). The category and the period fold away behind
// the Filters button on a phone or tablet (design rules S2 and S7).
export function FinanceFilterBar({
  filters,
  onChange,
  onClear,
  categories,
}: Props) {
  const types = (
    <SegmentedField
      label="Show"
      hideLabel
      joined
      options={typeOptions}
      value={filters.type}
      onChange={(type) => onChange(chooseType(filters, type, categories))}
    />
  );
  return (
    <FilterBar
      leading={types}
      active={countActive(filters)}
      filtered={financeFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="This week"
          options={periodOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      <FilterSelect
        label="Category"
        options={categoryOptions(categories, filters.type)}
        value={filters.category}
        onChange={(category) => onChange({ category })}
      />
    </FilterBar>
  );
}
