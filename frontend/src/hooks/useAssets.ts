import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { assetsService } from '../services/assetsService';
import { assetCategoriesService } from '../services/assetCategoriesService';
import type { Asset, AssetFilters } from '../types/asset';

export const useAssets = (filters: AssetFilters = {}, pageSize: number = 50, cursor?: string) => {
  return useQuery({
    queryKey: ['assets', filters, pageSize, cursor],
    queryFn: () => assetsService.list(filters, pageSize, cursor),
  });
};

export const useAsset = (id?: string) => {
  return useQuery({
    queryKey: ['assets', id],
    queryFn: () => (id ? assetsService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Asset>) => assetsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
    },
  });
};

export const useUpdateAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Asset> }) => assetsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
      queryClient.invalidateQueries({ queryKey: ['assets', variables.id] });
    },
  });
};

export const useDeleteAsset = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => assetsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assets'] });
    },
  });
};

// Asset Categories
export const useAssetCategories = () => {
  return useQuery({
    queryKey: ['assetCategories'],
    queryFn: () => assetCategoriesService.list(),
  });
};

export const useCreateAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string }) => assetCategoriesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assetCategories'] });
    },
  });
};

export const useUpdateAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: { name: string } }) =>
      assetCategoriesService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assetCategories'] });
    },
  });
};

export const useDeleteAssetCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => assetCategoriesService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assetCategories'] });
    },
  });
};
