import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { projectsService } from "../services/projectsService";
import type {
  ProjectCreatePayload,
  ProjectUpdatePayload,
  ProjectFilters,
} from "../types/project";

export const useProjects = (
  filters: ProjectFilters = {},
  pageSize: number = 24,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["projects", filters, pageSize, cursor],
    queryFn: () => projectsService.list(filters, pageSize, cursor),
  });
};

export const useProject = (id?: string) => {
  return useQuery({
    queryKey: ["projects", id],
    queryFn: () => projectsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ProjectCreatePayload) => projectsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["projects", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};

export const useUpdateProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ProjectUpdatePayload }) =>
      projectsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["projects", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};

export const useDeleteProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projectsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["projects", id] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
    },
  });
};
