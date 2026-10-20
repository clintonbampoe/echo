// What the projects-at-risk card says. Pure.
import { formatMoney } from "@/lib/format";
import type { ProjectTotals, TrackedProject } from "./api";

// One row: the project's name, and how much of what it needs has come in so far.
export type RiskRow = { key: string; name: string; note: string };

// "GH₵ 9,800 of GH₵ 30,000 raised". A project with no target just says what it has.
export function riskRows(projects: TrackedProject[] | undefined): RiskRow[] {
  return (projects ?? []).map((p) => ({
    key: p.id,
    name: p.name,
    note:
      p.target > 0
        ? `${formatMoney(p.raised)} of ${formatMoney(p.target)} raised`
        : `${formatMoney(p.raised)} raised`,
  }));
}

// The big figure on a phone: "3 of 5", the server's two counts. Nothing to say for a church with no
// projects, because nothing would be at risk anyway.
export const atRiskWords = (
  totals: ProjectTotals | undefined,
): string | undefined =>
  totals && totals.total > 0
    ? `${totals.atRisk} of ${totals.total}`
    : undefined;
