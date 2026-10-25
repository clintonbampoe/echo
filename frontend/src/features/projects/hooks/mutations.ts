import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createProjectCategory,
  deleteProjectCategory,
  mergeProjectCategory,
  updateProjectCategory,
} from "../api/categories";
import type { ContributionUpdate } from "../api/contributions";
import {
  createContribution,
  deleteContribution,
  updateContribution,
} from "../api/contributions";
import type { ProjectUpdate } from "../api/projects";
import { createProject, deleteProject, updateProject } from "../api/projects";
import { projectKeys } from "./keys";

// Adding, changing or deleting a project changes every list and number on Projects, the managers in the filter, the
// category counts, and what a member's page shows they manage. A contribution changes the amount raised, so it
// refreshes the same things. One refresh covers them all.
function useRefreshProjects() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: projectKeys.all });
}

export function useCreateProject() {
  const refresh = useRefreshProjects();
  return useMutation({ mutationFn: createProject, onSuccess: refresh });
}

export function useUpdateProject(id: string) {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: (input: ProjectUpdate) => updateProject(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteProject() {
  const refresh = useRefreshProjects();
  return useMutation({ mutationFn: deleteProject, onSuccess: refresh });
}

export function useCreateContribution() {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: createContribution,
    onSuccess: refresh,
  });
}

export function useUpdateContribution(id: string) {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: (input: ContributionUpdate) => updateContribution(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteContribution() {
  const refresh = useRefreshProjects();
  return useMutation({
    mutationFn: deleteContribution,
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
    mutationFn: createProjectCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      updateProjectCategory(id, name),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteProjectCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: deleteProjectCategory,
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
    }) => mergeProjectCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
