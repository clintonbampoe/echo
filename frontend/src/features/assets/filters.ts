// What the Assets page is narrowed by. The mechanics (reading and writing the address) are shared in lib; this file
// says which filters assets have and how they become API filters. No React in this file.
import { calendarYearDates } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { withAny, type Option } from "@/ui";
import type { AssetCategory, AssetFilters, AssetStatus } from "./api";
import { statusLabel } from "./labels";

export const assetStatuses: readonly AssetStatus[] = [
  "Active",
  "InUse",
  "InStorage",
  "UnderMaintenance",
  "Liquidated",
];

// The days of purchase the page covers. Everything the church owns is shown at first, however long ago it was bought.
export type PeriodKey = "all" | "year" | "last-year";

export const periodKeys: readonly PeriodKey[] = ["all", "year", "last-year"];

export const periodOptions: readonly { value: PeriodKey; label: string }[] = [
  { value: "all", label: "All time" },
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
];

export const statusOptions: readonly Option[] = withAny(
  "Status",
  assetStatuses.map((s) => ({ value: s, label: statusLabel[s] })),
);

export const assetFilters = urlFilters(
  { name: "", status: "", category: "", window: "all" },
  { status: ["", ...assetStatuses], window: periodKeys },
);

export type ListFilters = typeof assetFilters.empty;

export const statusOf = (value: string): AssetStatus | undefined =>
  assetStatuses.find((s) => s === value);

// The category a filter value names. Ids are numbers in the API and text in the address.
export const categoryOf = (value: string): number | undefined =>
  /^\d+$/.test(value) ? Number(value) : undefined;

export const categoryOptions = (
  categories: AssetCategory[] | undefined,
): readonly Option[] =>
  withAny(
    "Category",
    (categories ?? []).map((c) => ({ value: String(c.id), label: c.name })),
  );

export const periodDates = calendarYearDates;

// Filters narrowing the list, not counting the search box.
export const countActive = (f: ListFilters) =>
  (f.status ? 1 : 0) + (f.category ? 1 : 0) + (f.window === "all" ? 0 : 1);

export function toApiFilters(f: ListFilters, today: string): AssetFilters {
  return {
    name: f.name || undefined,
    status: statusOf(f.status),
    categoryId: categoryOf(f.category),
    ...periodDates(f.window, today),
  };
}
