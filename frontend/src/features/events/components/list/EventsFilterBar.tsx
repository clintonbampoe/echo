import { useOrganizationChoices } from "@/features/organizations";
import { windowOptions } from "@/lib/windows";
import { FilterBar as Bar, FilterSelect } from "@/ui";
import { withAny } from "@/lib/options";
import {
  countActive,
  eventFilters,
  type ListFilters,
} from "../../model/filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
};

// The Events list's search, organization and time range: the shared bar. The time range sits at the far right on a
// wide screen; on a phone or tablet everything but the search folds behind the "Filters" button.
export function EventsFilterBar({
  filters,
  onChange,
  resetKey,
  onClear,
}: Props) {
  const organizations = useOrganizationChoices();
  const options = withAny(
    "Organization",
    organizations.map((o) => ({ value: o.id, label: o.name })),
  );
  return (
    <Bar
      searchLabel="Search events"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={eventFilters.isFiltered(filters)}
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
        label="Organization"
        options={options}
        value={filters.organization}
        onChange={(organization) => onChange({ organization })}
      />
    </Bar>
  );
}
