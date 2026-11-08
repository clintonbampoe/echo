import { formatNumber, plural } from "@/lib/format";
import { roleLabel, type UserRole } from "@/features/account";
import type { StaffSummary } from "../api/staff";
import { countOf } from "./roles";

// "24 people · 1 disabled": the line under the title. Dashes would be noise, so it is empty while loading.
export function staffLine(s: StaffSummary | undefined): string | undefined {
  if (!s) return undefined;
  const people = plural(s.total, "person", "people");
  return s.disabled > 0
    ? `${people} · ${formatNumber(s.disabled)} disabled`
    : people;
}

// A role's share of staff as whole percent, "–" while the numbers load.
export function shareOf(count: number | undefined, total: number): string {
  if (count === undefined || total === 0) return "–";
  return `${Math.round((count / total) * 100)}%`;
}

// The centre of the roles ring: everyone, or the picked role's head count and share.
export type RingCentre = { label?: string; value: string; note: string };

export function ringCentre(
  summary: StaffSummary | undefined,
  role: UserRole | null,
): RingCentre {
  const total = summary?.total ?? 0;
  if (!summary) return { value: "–", note: "people on staff" };
  const count = role ? countOf(summary.roles, role) : undefined;
  if (!role || count === undefined)
    return { value: formatNumber(total), note: "people on staff" };
  return {
    label: roleLabel(role),
    value: formatNumber(count),
    note: `${shareOf(count, total)} of staff`,
  };
}
