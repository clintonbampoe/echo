import { apiFetch } from './api';
import type { AssetCategory } from '../types/asset';

const BASE_PATH = '/v1/AssetCategories';

export const assetCategoriesService = {
  list: async (): Promise<AssetCategory[]> => {
    return apiFetch(BASE_PATH);
  },

  getById: async (id: number): Promise<AssetCategory> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string }): Promise<AssetCategory> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: number, data: { name: string }): Promise<AssetCategory> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: async (id: number): Promise<void> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },

  search: async (name: string): Promise<AssetCategory[]> => {
    return apiFetch(`${BASE_PATH}/search?name=${encodeURIComponent(name)}`);
  },
};
