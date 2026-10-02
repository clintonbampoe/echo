import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { tithesService } from "../services/tithesService";
import type {
  TitheFilters,
  TitheCreatePayload,
  TitheUpdatePayload,
} from "../types/tithe";

export const useTithes = (
  filters: TitheFilters = {},
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["tithes", filters, pageSize, cursor],
    queryFn: () => tithesService.list(filters, pageSize, cursor),
  });
};

export const useTithe = (id?: string) => {
  return useQuery({
    queryKey: ["tithes", id],
    queryFn: () => tithesService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: TitheCreatePayload) => tithesService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["tithes", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["tithes"] });
    },
  });
};

export const useUpdateTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TitheUpdatePayload }) =>
      tithesService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["tithes", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["tithes"] });
    },
  });
};

export const useDeleteTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tithesService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["tithes", id] });
      queryClient.invalidateQueries({ queryKey: ["tithes"] });
    },
  });
};
