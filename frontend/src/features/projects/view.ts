// What the Projects page shows of its projects: the numbers, the progress and the dates. Pure, so the tests can
// check it without a screen.
import { formatMoney, formatMonth } from "@/lib/format";
import { withAny, type Option, type Stat } from "@/ui";
import type { Project, ProjectSummary } from "./api";

// How much of the target is raised, as a whole percent, rounded down so 100% means the target is really reached
// (99.6% reads 99%). It may pass 100: the target was beaten.
export const percentRaised = (
  p: Pick<Project, "raisedAmount" | "targetAmount">,
): number =>
  p.targetAmount > 0 ? Math.floor((p.raisedAmount / p.targetAmount) * 100) : 0;

// "Mar 2026 to Dec 2026", or "Mar 2026 onward" for a project with no end date.
export const projectDates = (
  p: Pick<Project, "startDate" | "endDate">,
): string =>
  p.endDate
    ? `${formatMonth(p.startDate)} to ${formatMonth(p.endDate)}`
    : `${formatMonth(p.startDate)} onward`;

// The four numbers above the list. `period` says which days they cover ("This year").
export function projectStats(
  summary: ProjectSummary | undefined,
  period: string,
): Stat[] {
  const s = summary;
  return [
    {
      label: "Projects",
      value: s ? String(s.totalProjects) : "–",
      note: period,
    },
    {
      label: "Raised",
      value: s ? formatMoney(s.totalRaised) : "–",
      note: "Across these projects",
    },
    {
      label: "Target",
      value: s ? formatMoney(s.totalTarget) : "–",
      note: "Across these projects",
    },
    {
      label: "At risk",
      value: s ? String(s.atRiskCount) : "–",
      note: "Marked at risk",
    },
  ];
}

// Each person who manages a project, once, by name. The API has no list of managers, so it comes from the projects.
export function managerOptions(
  projects: readonly Project[] | undefined,
): readonly Option[] {
  const seen = new Map<string, string>();
  for (const p of projects ?? []) seen.set(p.managerId, p.managerName);
  const people = [...seen]
    .map(([value, label]) => ({ value, label }))
    .sort((a, b) => a.label.localeCompare(b.label));
  return withAny("Manager", people);
}
