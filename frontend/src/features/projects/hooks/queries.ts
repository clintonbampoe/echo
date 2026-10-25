import { useQueries, useQuery } from "@tanstack/react-query";
import type { PaymentMethod } from "@/features/transactions";
import { ALL_TIME } from "@/lib/dates";
import { usePagedList } from "@/lib/paged";
import { listProjectCategories } from "../api/categories";
import type { ContributionFilters } from "../api/contributions";
import {
  getContributionSummary,
  listContributions,
} from "../api/contributions";
import type { ProjectFilters } from "../api/projects";
import {
  getProject,
  getProjectSummary,
  listManagedProjects,
  listProjects,
} from "../api/projects";
import { projectKeys } from "./keys";

const FIVE_MINUTES = 5 * 60_000;

// Every project one person manages.
export const useManagedProjects = (managerId: string) =>
  usePagedList(projectKeys.managed(managerId), (c, s) =>
    listManagedProjects(managerId, c, s),
  );

// Every project that matches the filters, newest start first.
export const useProjects = (filters: ProjectFilters) =>
  usePagedList(projectKeys.list(filters), (c, s) =>
    listProjects(filters, c, s),
  );

export const useProjectSummary = (filters: ProjectFilters) =>
  useQuery({
    queryKey: projectKeys.summary(filters),
    queryFn: ({ signal }) => getProjectSummary(filters, signal),
  });

// One project, for its own page.
export const useProject = (id: string) =>
  useQuery({
    queryKey: projectKeys.one(id),
    queryFn: ({ signal }) => getProject(id, signal),
  });

// The gifts towards one project that match the filters, newest first.
export const useContributions = (filters: ContributionFilters) =>
  usePagedList(projectKeys.contributions(filters), (c, s) =>
    listContributions(filters, c, s),
  );

export const useContributionSummary = (filters: ContributionFilters) =>
  useQuery({
    queryKey: projectKeys.contributionSummary(filters),
    queryFn: ({ signal }) => getContributionSummary(filters, signal),
  });

// What each way of giving brought in towards one project, for "How it was given". It asks the summary once per method,
// so the split follows the project's own numbers without the API needing a new endpoint.
export const useContributionsByMethod = (
  filters: ContributionFilters,
  methods: readonly PaymentMethod[],
) =>
  useQueries({
    queries: methods.map((paymentMethod) => {
      const f = { ...filters, paymentMethod };
      return {
        queryKey: projectKeys.contributionSummary(f),
        queryFn: ({ signal }: { signal: AbortSignal }) =>
          getContributionSummary(f, signal),
        select: (s: { totalContributed: number }) => ({
          method: paymentMethod,
          total: s.totalContributed,
        }),
      };
    }),
  });

// Every category, for the filter and (later) the form. They change rarely, so they are kept for a few minutes.
export const useProjectCategories = () =>
  useQuery({
    queryKey: projectKeys.categories,
    queryFn: ({ signal }) => listProjectCategories(signal),
    staleTime: FIVE_MINUTES,
  });

// The projects of all time (the first page), to work out who manages one: the API has no list of managers.
export const useProjectManagers = () =>
  useQuery({
    queryKey: projectKeys.managers,
    queryFn: ({ signal }) => listProjects(ALL_TIME, undefined, signal),
    staleTime: FIVE_MINUTES,
  });
