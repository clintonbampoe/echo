import type { ContributionFilters } from "../api/contributions";
import type { ProjectFilters } from "../api/projects";

export const projectKeys = {
  all: ["projects"] as const,
  managed: (id: string) => ["projects", "managed", id] as const,
  list: (filters: ProjectFilters) => ["projects", "list", filters] as const,
  summary: (filters: ProjectFilters) =>
    ["projects", "summary", filters] as const,
  categories: ["projects", "categories"] as const,
  managers: ["projects", "managers"] as const,
  one: (id: string) => ["projects", "one", id] as const,
  contributions: (filters: ContributionFilters) =>
    ["projects", "contributions", filters] as const,
  contributionSummary: (filters: ContributionFilters) =>
    ["projects", "contribution-summary", filters] as const,
};
