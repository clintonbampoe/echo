import { getProjectTotals, getProjects } from "../api/projects";
import { useOnce } from "./internals";

// Every project and what it has raised, all time.
export function useProjects() {
  const { data, failed, retry } = useOnce("projects", getProjects);
  return { projects: data, failed, retry };
}

// Of every project, how many are At Risk. No period.
export function useProjectTotals() {
  const { data, failed, retry } = useOnce("project-totals", getProjectTotals);
  return { totals: data, failed, retry };
}
