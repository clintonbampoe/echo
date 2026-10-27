// What the Projects page is narrowed by. The mechanics (reading and writing the address) are shared in lib;
// this file says which filters projects have and how they become API filters. No React in this file.
import { calendarYearDates } from "@/lib/dates";
import { urlFilters } from "@/lib/urlFilters";
import { type Option } from "@/ui";
import { withAny } from "@/lib/options";
import type { ProjectCategory } from "../api/categories";
import type { ProjectFilters, ProjectStatus } from "../api/projects";
import { statusLabel } from "./labels";

export const projectStatuses: readonly ProjectStatus[] = [
  "Planning",
  "OnTrack",
  "AtRisk",
  "Complete",
  "Missed",
];

// The days the page covers: projects running in them. The default is this year, as the server's is.
export type PeriodKey = "year" | "last-year" | "all";

export const periodKeys: readonly PeriodKey[] = ["year", "last-year", "all"];

export const periodOptions: readonly { value: PeriodKey; label: string }[] = [
  { value: "year", label: "This year" },
  { value: "last-year", label: "Last year" },
  { value: "all", label: "All time" },
];

export const statusOptions: readonly Option[] = withAny(
  "Status",
  projectStatuses.map((s) => ({ value: s, label: statusLabel[s] })),
);

export const projectFilters = urlFilters(
  { name: "", status: "", category: "", manager: "", window: "year" },
  { status: ["", ...projectStatuses], window: periodKeys },
);

export type ListFilters = typeof projectFilters.empty;

export const statusOf = (value: string): ProjectStatus | undefined =>
  projectStatuses.find((s) => s === value);

// The category a filter value names. Ids are numbers in the API and text in the address.
export const categoryOf = (value: string): number | undefined =>
  /^\d+$/.test(value) ? Number(value) : undefined;

export const categoryOptions = (
  categories: ProjectCategory[] | undefined,
): readonly Option[] =>
  withAny(
    "Category",
    (categories ?? []).map((c) => ({ value: String(c.id), label: c.name })),
  );

export const periodDates = calendarYearDates;

// Filters narrowing the list, not counting the search box.
export const countActive = (f: ListFilters) =>
  (f.status ? 1 : 0) +
  (f.category ? 1 : 0) +
  (f.manager ? 1 : 0) +
  (f.window === "year" ? 0 : 1);

export function toApiFilters(f: ListFilters, today: string): ProjectFilters {
  return {
    name: f.name || undefined,
    status: statusOf(f.status),
    categoryId: categoryOf(f.category),
    managerId: f.manager || undefined,
    ...periodDates(f.window, today),
  };
}
