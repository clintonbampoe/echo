import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { AssetUpdate } from "../api/assets";
import { createAsset, deleteAsset, updateAsset } from "../api/assets";
import {
  createAssetCategory,
  deleteAssetCategory,
  mergeAssetCategory,
  updateAssetCategory,
} from "../api/categories";
import { assetKeys } from "./keys";

// A new or deleted category changes only the category list. A rename also changes the name on every asset, and a merge
// moves assets, so those refresh everything on Assets.
function useRefreshCategories() {
  const client = useQueryClient();
  return (everything: boolean) =>
    void client.invalidateQueries({
      queryKey: everything ? assetKeys.all : assetKeys.categories,
    });
}

// Adding, changing or deleting an asset changes every list and number on Assets, and the category counts. One refresh
// covers them all.
function useRefreshAssets() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: assetKeys.all });
}

export function useCreateAsset() {
  const refresh = useRefreshAssets();
  return useMutation({ mutationFn: createAsset, onSuccess: refresh });
}

export function useUpdateAsset(id: string) {
  const refresh = useRefreshAssets();
  return useMutation({
    mutationFn: (input: AssetUpdate) => updateAsset(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteAsset() {
  const refresh = useRefreshAssets();
  return useMutation({ mutationFn: deleteAsset, onSuccess: refresh });
}

export function useCreateAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: createAssetCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      updateAssetCategory(id, name),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteAssetCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: deleteAssetCategory,
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
    }) => mergeAssetCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
