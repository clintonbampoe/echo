import { api, type DateWindow, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";

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
    query: { ...filters, cursor, pageSize: pageSize() },
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
