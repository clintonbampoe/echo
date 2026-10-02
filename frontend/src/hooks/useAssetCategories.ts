import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { assetCategoriesService } from "../services/assetCategoriesService";
import type {
  AssetCategoryCreatePayload,
  AssetCategoryUpdatePayload,
} from "../types/assetCategory";

export const useAssetCategories = () => {
  return useQuery({
    queryKey: ["assetCategories"],
    queryFn: () => assetCategoriesService.list(),
  });
};

export const useAssetCategory = (id?: number) => {
  return useQuery({
    queryKey: ["assetCategories", id],
    queryFn: () => assetCategoriesService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AssetCategoryCreatePayload) =>
      assetCategoriesService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["assetCategories", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["assetCategories"] });
    },
  });
};

export const useUpdateAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: AssetCategoryUpdatePayload;
    }) => assetCategoriesService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["assetCategories", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["assetCategories"] });
    },
  });
};

export const useDeleteAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => assetCategoriesService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["assetCategories", id] });
      queryClient.invalidateQueries({ queryKey: ["assetCategories"] });
    },
  });
};
