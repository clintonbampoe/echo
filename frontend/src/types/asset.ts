export type AssetStatus =
  "Active" | "InUse" | "InStorage" | "UnderMaintenance" | "Liquidated";

export interface Asset {
  id: string;
  categoryId: number;
  categoryName: string;
  name: string;
  serialNumber?: string | null;
  purchaseDate?: string | null;
  purchaseCost: number;
  currentValue: number;
  status: AssetStatus;
  description?: string | null;
  createdAt: string;
}

export interface AssetCreatePayload {
  categoryId: number;
  name: string;
  purchaseCost: number;
  currentValue: number;
  status: AssetStatus;
  serialNumber?: string | null;
  purchaseDate?: string | null;
  description?: string | null;
}

export type AssetUpdatePayload = Partial<AssetCreatePayload>;

export interface AssetSearchResult {
  id: string;
  name: string;
}

export interface AssetFilters {
  status?: AssetStatus;
  categoryId?: number;
  name?: string;
}
