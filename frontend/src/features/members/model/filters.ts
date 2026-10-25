// What the members list is narrowed by. The mechanics (reading and writing the address, the time range)
// are shared in lib; this file only says which filters members have. No React in this file.
import { withAny } from "@/lib/options";
import { regions } from "@/lib/regions";
import { urlFilters } from "@/lib/urlFilters";
import { windowDates, windowKeys, type WindowKey } from "@/lib/windows";
import type { MemberFilters } from "../api/members";
import { genderChoices, maritalChoices, statusChoices } from "./options";

export const memberFilters = urlFilters(
  {
    name: "",
    status: "",
    gender: "",
    region: "",
    maritalStatus: "",
    window: "year",
  },
  { window: windowKeys },
);

export type ListFilters = typeof memberFilters.empty;

// The time range as a WindowKey (the address can hold anything, and reading it falls back to this year).
export const windowOf = (f: ListFilters): WindowKey =>
  windowKeys.find((k) => k === f.window) ?? "year";

export const statusOptions = withAny("Any status", statusChoices);
export const genderOptions = withAny("Any gender", genderChoices);
export const regionOptions = withAny("Any region", regions);
export const maritalOptions = withAny("Any marital status", maritalChoices);

// Filters that are narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: ListFilters) =>
  [f.status, f.gender, f.region, f.maritalStatus].filter(Boolean).length +
  (f.window === "year" ? 0 : 1);

export function toApiFilters(f: ListFilters, now = new Date()): MemberFilters {
  return {
    name: f.name.trim() || undefined,
    status: (f.status || undefined) as MemberFilters["status"],
    gender: (f.gender || undefined) as MemberFilters["gender"],
    region: f.region || undefined,
    maritalStatus: (f.maritalStatus ||
      undefined) as MemberFilters["maritalStatus"],
    ...windowDates(f.window, now),
  };
}
