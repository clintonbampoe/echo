import { apiFetch } from './api';
import type { Asset, PagedResponse, AssetFilters, AssetCreatePayload, AssetUpdatePayload } from '../types/asset';

const BASE_PATH = '/v1/Assets';

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

  create: async (data: AssetCreatePayload): Promise<Asset> => {
    return apiFetch<Asset>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: AssetUpdatePayload): Promise<Asset> => {
    return apiFetch<Asset>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
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
