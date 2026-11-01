import type { TitheFilters, TitheMonthsFor } from "../api/tithes";

export const titheKeys = {
  all: ["tithes"] as const,
  list: (filters: TitheFilters) => ["tithes", "list", filters] as const,
  summary: (filters: TitheFilters) => ["tithes", "summary", filters] as const,
  member: (id: string, filters: TitheFilters) =>
    ["tithes", "member", id, filters] as const,
  one: (id: string) => ["tithes", "one", id] as const,
  sameMonth: (memberId: string, year: number, month: string) =>
    ["tithes", "same-month", memberId, year, month] as const,
  months: (who: TitheMonthsFor, months: number, to: string) =>
    ["tithes", "months", who, months, to] as const,
};
