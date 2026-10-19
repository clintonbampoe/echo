// The only file in assets that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

const FIVE_MINUTES = 5 * 60_000;

export const assetKeys = {
  all: ["assets"] as const,
  list: (filters: api.AssetFilters) => ["assets", "list", filters] as const,
  summary: (filters: api.AssetFilters) =>
    ["assets", "summary", filters] as const,
  categories: ["assets", "categories"] as const,
  one: (id: string) => ["assets", "one", id] as const,
};

// Every asset that matches the filters, by name.
export const useAssets = (filters: api.AssetFilters) =>
  usePagedList(assetKeys.list(filters), (c, s) =>
    api.listAssets(filters, c, s),
  );

export const useAssetSummary = (filters: api.AssetFilters) =>
  useQuery({
    queryKey: assetKeys.summary(filters),
    queryFn: ({ signal }) => api.getAssetSummary(filters, signal),
  });

// One asset, for its own page.
export const useAsset = (id: string) =>
  useQuery({
    queryKey: assetKeys.one(id),
    queryFn: ({ signal }) => api.getAsset(id, signal),
  });

// Every category, for the filter and the form. They change rarely, so they are kept for a few minutes.
export const useAssetCategories = () =>
  useQuery({
    queryKey: assetKeys.categories,
    queryFn: ({ signal }) => api.listAssetCategories(signal),
    staleTime: FIVE_MINUTES,
  });

// Adding, changing or deleting an asset changes every list and number on Assets, and the category counts. One refresh
// covers them all.
function useRefreshAssets() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: assetKeys.all });
}

export function useCreateAsset() {
  const refresh = useRefreshAssets();
  return useMutation({ mutationFn: api.createAsset, onSuccess: refresh });
}

export function useUpdateAsset(id: string) {
  const refresh = useRefreshAssets();
  return useMutation({
    mutationFn: (input: api.AssetUpdate) => api.updateAsset(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteAsset() {
  const refresh = useRefreshAssets();
  return useMutation({ mutationFn: api.deleteAsset, onSuccess: refresh });
}

// A new or deleted category changes only the category list. A rename also changes the name on every asset, and a merge
// moves assets, so those refresh everything on Assets.
function useRefreshCategories() {
  const client = useQueryClient();
  return (everything: boolean) =>
    void client.invalidateQueries({
      queryKey: everything ? assetKeys.all : assetKeys.categories,
    });
}

export function useCreateAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: api.createAssetCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      api.updateAssetCategory(id, name),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: api.deleteAssetCategory,
    onSuccess: () => refresh(false),
  });
}

export function useMergeAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({
      id,
      targetCategoryId,
    }: {
      id: number;
      targetCategoryId: number;
    }) => api.mergeAssetCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
