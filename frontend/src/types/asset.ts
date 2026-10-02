export type AssetStatus = 'Active' | 'InUse' | 'InStorage' | 'UnderMaintenance' | 'Liquidated';

export interface AssetCategory {
  id: number;
  name: string;
}

export interface AssetCreatePayload {
  categoryId: number;
  name: string;
  purchaseCost: number;
  currentValue: number;
  status: AssetStatus;
  serialNumber?: string | null;
  purchaseDate?: string | null; // ISO format YYYY-MM-DD
  description?: string | null;
}

export interface AssetUpdatePayload {
  categoryId?: number;
  name?: string;
  purchaseCost?: number;
  currentValue?: number;
  status?: AssetStatus;
  serialNumber?: string | null;
  purchaseDate?: string | null;
  description?: string | null;
}

export interface Asset {
  id: string;
  categoryId: number;
  categoryName: string;
  name: string;
  serialNumber?: string | null;
  purchaseDate?: string | null; // ISO format YYYY-MM-DD
  purchaseCost: number;
  currentValue: number;
  status: AssetStatus;
  description?: string | null;
  createdAt: string;
}

export interface AssetFilters {
  status?: AssetStatus;
  categoryId?: number;
  name?: string;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
