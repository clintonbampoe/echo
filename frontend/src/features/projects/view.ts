// What the Projects page shows of its projects: the numbers, the progress and the dates. Pure, so the tests can
// check it without a screen.
import { formatMonth, formatNumber } from "@/lib/format";
import { daysBetween } from "@/lib/dates";
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
  const funded =
    s && s.totalTarget > 0 ? (s.totalRaised / s.totalTarget) * 100 : 0;
  const average =
    s && s.totalProjects > 0 ? s.totalRaised / s.totalProjects : 0;
  return [
    {
      label: "Raised",
      value: s ? formatNumber(s.totalRaised) : "–",
      unit: "GHS",
      note: period,
    },
    {
      label: "Goal",
      value: s ? formatNumber(s.totalTarget) : "–",
      unit: "GHS",
      note: `Across ${s?.totalProjects ?? 0} projects`,
    },
    {
      label: "Funded",
      value: s ? String(Math.floor(funded)) : "–",
      unit: "%",
      note: s && s.atRiskCount > 0 ? `${s.atRiskCount} at risk` : "",
    },
    {
      label: "Average raised",
      value: s ? formatNumber(Math.round(average)) : "–",
      unit: "GHS",
      note: "Per project",
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

// The four numbers on a project's page. "Days left" counts to the end date and stops at 0; with no end date it is a dash.
export function projectFigures(
  p: Pick<Project, "raisedAmount" | "targetAmount" | "endDate">,
  contributions: number | undefined,
  today: string,
): Stat[] {
  const left = p.endDate ? Math.max(0, daysBetween(today, p.endDate)) : null;
  return [
    {
      label: "Raised",
      value: formatNumber(p.raisedAmount),
      unit: "GHS",
      note: "",
    },
    {
      label: "Goal",
      value: formatNumber(p.targetAmount),
      unit: "GHS",
      note: "",
    },
    {
      label: "Contributions",
      value: contributions === undefined ? "–" : formatNumber(contributions),
      note: "",
    },
    {
      label: "Days left",
      value: left === null ? "–" : formatNumber(left),
      note: "",
    },
  ];
}
