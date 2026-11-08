import { formatNumber, plural } from "@/lib/format";
import type { TeamSummary } from "../api/team";

// "24 people · 1 disabled": the line under the title. Dashes would be noise, so it is empty while loading.
export function teamLine(s: TeamSummary | undefined): string | undefined {
  if (!s) return undefined;
  const people = plural(s.total, "person", "people");
  return s.disabled > 0
    ? `${people} · ${formatNumber(s.disabled)} disabled`
    : people;
}

// A role's share of the team as whole percent, "–" while the numbers load.
export function shareOf(count: number | undefined, total: number): string {
  if (count === undefined || total === 0) return "–";
  return `${Math.round((count / total) * 100)}%`;
}
