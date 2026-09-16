import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tithesService } from '../services/tithesService';
import type { Tithe, TitheFilters } from '../types/finance';

export const useTithes = (filters: TitheFilters = {}, pageSize: number = 50, cursor?: string) => {
  return useQuery({
    queryKey: ['tithes', filters, pageSize, cursor],
    queryFn: () => tithesService.list(filters, pageSize, cursor),
  });
};

export const useTithe = (id?: string) => {
  return useQuery({
    queryKey: ['tithes', id],
    queryFn: () => (id ? tithesService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Tithe>) => tithesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tithes'] });
    },
  });
};

export const useUpdateTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Tithe> }) => tithesService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tithes'] });
      queryClient.invalidateQueries({ queryKey: ['tithes', variables.id] });
    },
  });
};

export const useDeleteTithe = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tithesService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tithes'] });
    },
  });
};
