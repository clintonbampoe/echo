import { FilterBar, FilterSelect, MonthYearPill } from "@/ui";
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
  today: string;
};

// The Finance filters: the month, the type and the category as pills, and the days to cover at the far right. The
// server cannot search transactions, so there is no search box. On a phone or tablet they fold behind Filters.
export function FinanceFilterBar({
  filters,
  onChange,
  onClear,
  categories,
  today,
}: Props) {
  return (
    <FilterBar
      active={countActive(filters)}
      filtered={financeFilters.isFiltered(filters)}
      onClear={onClear}
      trailing={
        <FilterSelect
          label="Range"
          options={periodOptions}
          value={filters.window}
          quiet
          onChange={(window) => onChange({ window })}
        />
      }
    >
      <MonthYearPill
        label="Month"
        value={filters.month}
        today={today}
        onChange={(month) => onChange({ month })}
      />
      <FilterSelect
        label="Type"
        options={typeOptions}
        value={filters.type}
        onChange={(type) => onChange(chooseType(filters, type, categories))}
      />
      <FilterSelect
        label="Category"
        options={categoryOptions(categories, filters.type)}
        value={filters.category}
        onChange={(category) => onChange({ category })}
      />
    </FilterBar>
  );
}
