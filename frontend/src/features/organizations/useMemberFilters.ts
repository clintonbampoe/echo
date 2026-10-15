import { useState } from "react";
import type { OrganizationMemberFilters } from "./api";
import { toMemberFilters } from "./view";

// What narrows the members list: a role and a name search. `resetKey` changes on every clear: give it as the
// `key` of the search box so the box empties too.
export function useMemberFilters() {
  const [role, setRole] = useState("");
  const [name, setName] = useState("");
  const [resetKey, setResetKey] = useState(0);
  const api: OrganizationMemberFilters = toMemberFilters(role, name);
  return {
    role,
    name,
    resetKey,
    api,
    filtered: api.role !== undefined || api.name !== undefined,
    setRole,
    setName,
    clear: () => {
      setRole("");
      setName("");
      setResetKey(resetKey + 1);
    },
  };
}
