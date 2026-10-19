// The asset endpoints: what the church owns. No React in this file.
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

export type AssetStatus =
  "Active" | "InUse" | "InStorage" | "UnderMaintenance" | "Liquidated";

export type Asset = {
  id: string;
  name: string;
  categoryId: number;
  categoryName: string;
  serialNumber?: string | null;
  // The day it was bought. An asset can have none.
  purchaseDate?: string | null;
  purchaseCost: number;
  // What it is worth now. Typed in by hand: nothing depreciates it.
  currentValue: number;
  status: AssetStatus;
  description?: string | null;
  createdAt: string;
};

// What the Assets page narrows the list by. The dates are about the day of purchase. With no dates the server answers
// for this year only, so the page always sends them.
export type AssetFilters = {
  name?: string;
  status?: AssetStatus;
  categoryId?: number;
  from: string;
  to: string;
};

export const getAsset = (id: string, signal?: AbortSignal) =>
  api.get<Asset>(`/v1/assets/${id}`, { signal });

export const listAssets = (
  filters: AssetFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Asset>>("/v1/assets", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// The numbers above the list. They follow the same filters, but leave liquidated assets out (they are no longer owned)
// unless the status filter asks for them.
export type AssetSummary = {
  totalAssets: number;
  totalCurrentValue: number;
  totalPurchaseCost: number;
  // Cost minus current value. Below zero when the assets are worth more than they cost.
  totalDepreciation: number;
  window?: DateWindow | null;
};

export const getAssetSummary = (filters: AssetFilters, signal?: AbortSignal) =>
  api.get<AssetSummary>("/v1/assets/summary", {
    query: { ...filters },
    signal,
  });

// What assets are grouped under, with how many each holds.
export type AssetCategory = { id: number; name: string; assetCount: number };

export const listAssetCategories = (signal?: AbortSignal) =>
  api.get<AssetCategory[]>("/v1/asset-categories", { signal });

// What a new asset is made of. A date is a day ("2023-03-12"); the serial number, date and description are left out
// when there are none.
export type AssetInput = {
  categoryId: number;
  name: string;
  status: AssetStatus;
  serialNumber?: string;
  purchaseDate?: string;
  purchaseCost: number;
  currentValue: number;
  description?: string;
};

// Saving changes sends only what is wanted changed. A purchase date can be changed but not taken away.
export type AssetUpdate = Partial<AssetInput>;

export const createAsset = (input: AssetInput) =>
  api.post<Asset>("/v1/assets", input);

export const updateAsset = (id: string, input: AssetUpdate) =>
  api.put<Asset>(`/v1/assets/${id}`, input);

// Soft delete: the asset leaves the list and the totals, and the record is kept.
export const deleteAsset = (id: string) => api.delete<void>(`/v1/assets/${id}`);

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
