// What staff is narrowed by. The mechanics are shared in lib; this file only says which filters staff has.
import { urlFilters } from "@/lib/urlFilters";
import type { StaffQuery, StaffStatus } from "../api/staff";

const statuses = ["Active", "Disabled", "Unverified"] as const;

// `q` is the search box: text in a name or email.
export const staffFilters = urlFilters(
  { q: "", status: "" },
  { status: ["", ...statuses] },
);

export type StaffFilters = typeof staffFilters.empty;

export const statusOptions = [
  { value: "", label: "All people" },
  ...statuses.map((s) => ({ value: s, label: s })),
];

// What the lists ask the server for.
export const toQuery = (f: StaffFilters): StaffQuery => ({
  status: f.status === "" ? undefined : (f.status as StaffStatus),
  q: f.q.trim() || undefined,
});

// Filters narrowing the list, not counting the search box (which is always visible).
export const countActive = (f: StaffFilters) => (f.status ? 1 : 0);
