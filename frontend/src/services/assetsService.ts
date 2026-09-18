import { apiFetch } from './api';
import type { Asset, PagedResponse, AssetFilters } from '../types/asset';

const BASE_PATH = '/v1/Assets';

function cleanAssetPayload(data: Partial<Asset>) {
  const payload: Record<string, unknown> = { ...data };

  delete payload.id;
  delete payload.categoryName;
  delete payload.createdAt;

  if (payload.categoryId) {
    payload.categoryId = Number(payload.categoryId);
  }

  if (payload.purchaseCost !== undefined) {
    payload.purchaseCost = Number(payload.purchaseCost);
  }

  if (payload.currentValue !== undefined) {
    payload.currentValue = Number(payload.currentValue);
  }

  if (payload.purchaseDate && typeof payload.purchaseDate === 'string') {
    payload.purchaseDate = payload.purchaseDate.split('T')[0];
  }

  if (!payload.serialNumber || (typeof payload.serialNumber === 'string' && payload.serialNumber.trim() === '')) {
    delete payload.serialNumber;
  }

  if (!payload.description || (typeof payload.description === 'string' && payload.description.trim() === '')) {
    delete payload.description;
  }

  return payload;
}

export const assetsService = {
  list: async (
    filters: AssetFilters = {},
    pageSize: number = 50,
    cursor?: string
  ): Promise<PagedResponse<Asset>> => {
    const params = new URLSearchParams();
    if (filters.status) params.append('Status', filters.status);
    if (filters.categoryId) params.append('CategoryId', filters.categoryId.toString());
    if (filters.name) params.append('Name', filters.name);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch<PagedResponse<Asset>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Asset> => {
    return apiFetch<Asset>(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Asset>): Promise<Asset> => {
    const payload = cleanAssetPayload(data);
    return apiFetch<Asset>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Asset>): Promise<Asset> => {
    const payload = cleanAssetPayload(data);
    return apiFetch<Asset>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },

  search: async (name: string): Promise<Asset[]> => {
    return apiFetch<Asset[]>(`${BASE_PATH}/search?name=${encodeURIComponent(name)}`);
  },
};
