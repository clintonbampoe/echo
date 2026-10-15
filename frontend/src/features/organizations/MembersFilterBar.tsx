import { FilterBar, FilterSelect } from "@/ui";
import { roleOptions } from "./options";
import type { useMemberFilters } from "./useMemberFilters";

type Props = { filters: ReturnType<typeof useMemberFilters> };

// The members list's search and filters: the shared bar, so on a phone or tablet the filters fold behind the
// "Filters" button exactly as on Members and Visitors, even though there is only one for now.
export function MembersFilterBar({ filters: f }: Props) {
  return (
    <FilterBar
      searchLabel="Search this group"
      search={f.name}
      onSearch={f.setName}
      resetKey={f.resetKey}
      active={f.role === "" ? 0 : 1}
      filtered={f.filtered}
      onClear={f.clear}
    >
      <FilterSelect
        label="Role"
        options={roleOptions}
        value={f.role}
        onChange={f.setRole}
      />
    </FilterBar>
  );
}
