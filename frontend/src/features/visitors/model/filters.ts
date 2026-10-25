// What the visitors list is narrowed by. The mechanics (reading and writing the address, the time range)
// are shared in lib; this file only says which filters visitors have. No React in this file.
import { urlFilters } from "@/lib/urlFilters";
import { windowDates, windowKeys } from "@/lib/windows";
import type { VisitorFilters } from "../api/visitors";

export const visitorFilters = urlFilters(
  { name: "", status: "", window: "year" },
  { status: ["", "became"], window: windowKeys },
);

export type ListFilters = typeof visitorFilters.empty;

// Filters that are narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: ListFilters) =>
  (f.status ? 1 : 0) + (f.window === "year" ? 0 : 1);

export function toApiFilters(f: ListFilters, now = new Date()): VisitorFilters {
  return {
    name: f.name.trim() || undefined,
    converted: f.status === "became" ? true : undefined,
    ...windowDates(f.window, now),
  };
}

// The summary follows the time range only.
export const toSummaryFilters = (f: ListFilters, now = new Date()) =>
  windowDates(f.window, now);
