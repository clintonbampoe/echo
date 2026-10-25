import { api } from "@/lib/api";

// What assets are grouped under, with how many each holds.
export type AssetCategory = { id: number; name: string; assetCount: number };

export const listAssetCategories = (signal?: AbortSignal) =>
  api.get<AssetCategory[]>("/v1/asset-categories", { signal });

// A new category is only a name. Saving changes sends the name too: it is all there is to change.
export const createAssetCategory = (name: string) =>
  api.post<AssetCategory>("/v1/asset-categories", { name });

export const updateAssetCategory = (id: number, name: string) =>
  api.put<AssetCategory>(`/v1/asset-categories/${id}`, { name });

// A category in use cannot be deleted: the server answers with a conflict.
export const deleteAssetCategory = (id: number) =>
  api.delete<void>(`/v1/asset-categories/${id}`);

// How many assets moved, and whether the old category went with them.
export type ReassignResult = { moved: number; sourceDeleted: boolean };

// Moves every asset of one category to another, and deletes the first.
export const mergeAssetCategory = (id: number, targetCategoryId: number) =>
  api.post<ReassignResult>(`/v1/asset-categories/${id}/reassign`, {
    targetCategoryId,
    deleteSource: true,
  });
