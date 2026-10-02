import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { assetsService } from "../services/assetsService";
import type {
  AssetFilters,
  AssetCreatePayload,
  AssetUpdatePayload,
} from "../types/asset";

export const useAssets = (
  filters: AssetFilters = {},
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["assets", filters, pageSize, cursor],
    queryFn: () => assetsService.list(filters, pageSize, cursor),
  });
};

export const useAsset = (id?: string) => {
  return useQuery({
    queryKey: ["assets", id],
    queryFn: () => assetsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AssetCreatePayload) => assetsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["assets", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["assets"] });
    },
  });
};

export const useUpdateAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssetUpdatePayload }) =>
      assetsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["assets", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["assets"] });
    },
  });
};

export const useDeleteAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => assetsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["assets", id] });
      queryClient.invalidateQueries({ queryKey: ["assets"] });
    },
  });
};
