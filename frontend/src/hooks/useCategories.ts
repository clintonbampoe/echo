import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { categoriesService, CategoryType } from '../services/categoriesService';
import type { Category } from '../types/category';

export const useCategories = (type: CategoryType) => {
  return useQuery({
    queryKey: ['categories', type],
    queryFn: () => categoriesService.list(type),
    enabled: !!type,
  });
};

export const useCategory = (type: CategoryType, id: number) => {
  return useQuery({
    queryKey: ['categories', type, id],
    queryFn: () => categoriesService.getById(type, id),
    enabled: !!id && !!type,
  });
};

export const useCreateCategory = (type: CategoryType) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Category>) => categoriesService.create(type, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories', type] });
    },
  });
};

export const useUpdateCategory = (type: CategoryType) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<Category> }) => categoriesService.update(type, id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['categories', type] });
      queryClient.invalidateQueries({ queryKey: ['categories', type, variables.id] });
    },
  });
};

export const useDeleteCategory = (type: CategoryType) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => categoriesService.delete(type, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories', type] });
    },
  });
};
