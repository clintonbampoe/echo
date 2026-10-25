import type { VisitorFilters } from "../api/visitors";

// Every cache key visitors use, in one place, so a change can name exactly what it makes stale.
export const visitorKeys = {
  all: ["visitors"] as const,
  list: (filters: VisitorFilters) => ["visitors", "list", filters] as const,
  summary: (filters: Pick<VisitorFilters, "from" | "to">) =>
    ["visitors", "summary", filters] as const,
  one: (id: string) => ["visitors", "one", id] as const,
  search: (q: string) => ["visitors", "search", q] as const,
};
