import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { projectCategoriesService } from "../services/projectCategoriesService";
import type {
  ProjectCategoryCreatePayload,
  ProjectCategoryUpdatePayload,
} from "../types/projectCategory";

export const useProjectCategories = () => {
  return useQuery({
    queryKey: ["projectCategories"],
    queryFn: () => projectCategoriesService.list(),
  });
};

export const useProjectCategory = (id?: number) => {
  return useQuery({
    queryKey: ["projectCategories", id],
    queryFn: () => projectCategoriesService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateProjectCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ProjectCategoryCreatePayload) =>
      projectCategoriesService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["projectCategories", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["projectCategories"] });
    },
  });
};

export const useUpdateProjectCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: ProjectCategoryUpdatePayload;
    }) => projectCategoriesService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["projectCategories", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["projectCategories"] });
    },
  });
};

export const useDeleteProjectCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => projectCategoriesService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["projectCategories", id] });
      queryClient.invalidateQueries({ queryKey: ["projectCategories"] });
    },
  });
};
