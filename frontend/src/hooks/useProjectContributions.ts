import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { projectContributionsService } from "../services/projectContributionsService";
import type {
  ProjectContributionCreatePayload,
  ProjectContributionUpdatePayload,
  ProjectContributionFilters,
} from "../types/projectContribution";

export const useProjectContributions = (
  filters: ProjectContributionFilters = {},
  pageSize: number = 24,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["projectContributions", filters, pageSize, cursor],
    queryFn: () => projectContributionsService.list(filters, pageSize, cursor),
  });
};

export const useProjectContributionsByProject = (
  projectId: string,
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["projectContributions", "project", projectId, pageSize, cursor],
    queryFn: () =>
      projectContributionsService.listByProjectId(projectId, pageSize, cursor),
    enabled: !!projectId,
  });
};

export const useProjectContribution = (id?: string) => {
  return useQuery({
    queryKey: ["projectContributions", id],
    queryFn: () => projectContributionsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateProjectContribution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ProjectContributionCreatePayload) =>
      projectContributionsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["projectContributions", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["projectContributions"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};

export const useUpdateProjectContribution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: ProjectContributionUpdatePayload;
    }) => projectContributionsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["projectContributions", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["projectContributions"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};

export const useDeleteProjectContribution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projectContributionsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["projectContributions", id] });
      queryClient.invalidateQueries({ queryKey: ["projectContributions"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};
