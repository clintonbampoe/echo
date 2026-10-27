// What the organizations list is narrowed by. The mechanics (reading and writing the address, the time range)
// are shared in lib; this file only says which filters organizations have. No React in this file.
import { urlFilters } from "@/lib/urlFilters";
import { windowDates, windowKeys } from "@/lib/windows";
import type { OrganizationFilters } from "../api/organizations";

// The time range is by the day an organization was created. A church's groups are mostly older than this
// year, so the list starts at all time rather than hiding them.
export const organizationFilters = urlFilters(
  { name: "", window: "all" },
  { window: windowKeys },
);

export type ListFilters = typeof organizationFilters.empty;

// Filters that are narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: ListFilters) => (f.window === "all" ? 0 : 1);

export function toApiFilters(
  f: ListFilters,
  now = new Date(),
): OrganizationFilters {
  return { name: f.name.trim() || undefined, ...windowDates(f.window, now) };
}

// The summary follows the time range only.
export const toSummaryFilters = (f: ListFilters, now = new Date()) =>
  windowDates(f.window, now);
