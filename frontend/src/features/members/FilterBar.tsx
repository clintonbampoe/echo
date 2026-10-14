import { FilterBar as Bar, FilterSelect } from "@/ui";
import { windowOptions } from "@/lib/windows";
import {
  countActive,
  genderOptions,
  maritalOptions,
  memberFilters,
  regionOptions,
  statusOptions,
  type ListFilters,
} from "./filters";

type Props = {
  filters: ListFilters;
  onChange: (change: Partial<ListFilters>) => void;
  // Changes when the filters are cleared, so the search box empties too.
  resetKey: number;
  onClear: () => void;
};

// The members list's filters: the shared bar, with the pills that belong to members.
export function FilterBar({ filters, onChange, resetKey, onClear }: Props) {
  return (
    <Bar
      searchLabel="Search members"
      search={filters.name}
      onSearch={(name) => onChange({ name })}
      resetKey={resetKey}
      active={countActive(filters)}
      filtered={memberFilters.isFiltered(filters)}
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
        label="Status"
        options={statusOptions}
        value={filters.status}
        onChange={(status) => onChange({ status })}
      />
      <FilterSelect
        label="Gender"
        options={genderOptions}
        value={filters.gender}
        onChange={(gender) => onChange({ gender })}
      />
      <FilterSelect
        label="Region"
        options={regionOptions}
        value={filters.region}
        onChange={(region) => onChange({ region })}
      />
      <FilterSelect
        label="Marital status"
        options={maritalOptions}
        value={filters.maritalStatus}
        onChange={(maritalStatus) => onChange({ maritalStatus })}
      />
    </Bar>
  );
}
