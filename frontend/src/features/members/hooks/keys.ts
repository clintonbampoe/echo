import type { MemberFilters } from "../api/members";

// Every cache key members use, in one place, so a change can name exactly what it makes stale.
export const memberKeys = {
  all: ["members"] as const,
  list: (filters: MemberFilters) => ["members", "list", filters] as const,
  summary: (filters: MemberFilters) => ["members", "summary", filters] as const,
  search: (q: string) => ["members", "search", q] as const,
  one: (id: string) => ["members", "one", id] as const,
};
