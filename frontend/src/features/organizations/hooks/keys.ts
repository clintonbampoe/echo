import type { OrganizationMemberFilters } from "../api/memberships";
import type { OrganizationFilters } from "../api/organizations";

export const organizationKeys = {
  all: ["organizations"] as const,
  list: (filters: OrganizationFilters) =>
    ["organizations", "list", filters] as const,
  summary: (filters: Pick<OrganizationFilters, "from" | "to">) =>
    ["organizations", "summary", filters] as const,
  one: (id: string) => ["organizations", "one", id] as const,
  members: (id: string, filters: OrganizationMemberFilters) =>
    ["organizations", "members", id, filters] as const,
  person: (id: string) => ["organizations", "person", id] as const,
};
