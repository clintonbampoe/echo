// What the team is narrowed by. The mechanics are shared in lib; this file only says which filters the team has.
import { urlFilters } from "@/lib/urlFilters";
import type { TeamStatus } from "../api/team";

const statuses = ["Active", "Disabled", "Unverified"] as const;

export const teamFilters = urlFilters(
  { status: "" },
  { status: ["", ...statuses] },
);

export type TeamFilters = typeof teamFilters.empty;

export const statusOptions = [
  { value: "", label: "All people" },
  ...statuses.map((s) => ({ value: s, label: s })),
];

export const toStatus = (f: TeamFilters): TeamStatus | undefined =>
  f.status === "" ? undefined : (f.status as TeamStatus);

export const countActive = (f: TeamFilters) => (f.status ? 1 : 0);
