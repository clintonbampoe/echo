// The only file in projects that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

const FIVE_MINUTES = 5 * 60_000;

export const projectKeys = {
  all: ["projects"] as const,
  managed: (id: string) => ["projects", "managed", id] as const,
  list: (filters: api.ProjectFilters) => ["projects", "list", filters] as const,
  summary: (filters: api.ProjectFilters) =>
    ["projects", "summary", filters] as const,
  categories: ["projects", "categories"] as const,
  managers: ["projects", "managers"] as const,
  one: (id: string) => ["projects", "one", id] as const,
  contributions: (filters: api.ContributionFilters) =>
    ["projects", "contributions", filters] as const,
  contributionSummary: (filters: api.ContributionFilters) =>
    ["projects", "contribution-summary", filters] as const,
};

// Every project one person manages.
export const useManagedProjects = (managerId: string) =>
  usePagedList(projectKeys.managed(managerId), (c, s) =>
    api.listManagedProjects(managerId, c, s),
  );

// Every project that matches the filters, newest start first.
export const useProjects = (filters: api.ProjectFilters) =>
  usePagedList(projectKeys.list(filters), (c, s) =>
    api.listProjects(filters, c, s),
  );

export const useProjectSummary = (filters: api.ProjectFilters) =>
  useQuery({
    queryKey: projectKeys.summary(filters),
    queryFn: ({ signal }) => api.getProjectSummary(filters, signal),
  });

// One project, for its own page.
export const useProject = (id: string) =>
  useQuery({
    queryKey: projectKeys.one(id),
    queryFn: ({ signal }) => api.getProject(id, signal),
  });

// The gifts towards one project that match the filters, newest first.
export const useContributions = (filters: api.ContributionFilters) =>
  usePagedList(projectKeys.contributions(filters), (c, s) =>
    api.listContributions(filters, c, s),
  );

export const useContributionSummary = (filters: api.ContributionFilters) =>
  useQuery({
    queryKey: projectKeys.contributionSummary(filters),
    queryFn: ({ signal }) => api.getContributionSummary(filters, signal),
  });

// Every category, for the filter and (later) the form. They change rarely, so they are kept for a few minutes.
export const useProjectCategories = () =>
  useQuery({
    queryKey: projectKeys.categories,
    queryFn: ({ signal }) => api.listProjectCategories(signal),
    staleTime: FIVE_MINUTES,
  });

// The projects of all time (the first page), to work out who manages one: the API has no list of managers.
export const useProjectManagers = () =>
  useQuery({
    queryKey: projectKeys.managers,
    queryFn: ({ signal }) => api.listProjects(ALL_TIME, undefined, signal),
    staleTime: FIVE_MINUTES,
  });

// Adding, changing or deleting a project changes every list and number on Projects, the managers in the filter, the
// category counts, and what a member's page shows they manage. A contribution changes the amount raised, so it
// refreshes the same things. One refresh covers them all.
function useRefreshProjects() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: projectKeys.all });
}

export function useCreateProject() {
  const refresh = useRefreshProjects();
  return useMutation({ mutationFn: api.createProject, onSuccess: refresh });
}

export function useUpdateProject(id: string) {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: (input: api.ProjectUpdate) => api.updateProject(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteProject() {
  const refresh = useRefreshProjects();
  return useMutation({ mutationFn: api.deleteProject, onSuccess: refresh });
}

export function useCreateContribution() {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: api.createContribution,
    onSuccess: refresh,
  });
}

export function useUpdateContribution(id: string) {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: (input: api.ContributionUpdate) =>
      api.updateContribution(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteContribution() {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: api.deleteContribution,
    onSuccess: refresh,
  });
}

// A new or deleted category changes only the category list. A rename also changes the name on every project, and a
// merge moves projects, so those refresh everything on Projects.
function useRefreshCategories() {
  const client = useQueryClient();
  return (everything: boolean) =>
    void client.invalidateQueries({
      queryKey: everything ? projectKeys.all : projectKeys.categories,
    });
}

export function useCreateProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: api.createProjectCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      api.updateProjectCategory(id, name),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: api.deleteProjectCategory,
    onSuccess: () => refresh(false),
  });
}

export function useMergeProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({
      id,
      targetCategoryId,
    }: {
      id: number;
      targetCategoryId: number;
    }) => api.mergeProjectCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
