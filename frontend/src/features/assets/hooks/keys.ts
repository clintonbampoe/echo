import type { AssetFilters } from "../api/assets";

export const assetKeys = {
  all: ["assets"] as const,
  list: (filters: AssetFilters) => ["assets", "list", filters] as const,
  summary: (filters: AssetFilters) => ["assets", "summary", filters] as const,
  categories: ["assets", "categories"] as const,
  one: (id: string) => ["assets", "one", id] as const,
};
