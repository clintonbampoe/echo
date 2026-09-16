import { apiFetch } from './api';
import type { Organization, PagedResponse } from '../types/event';

const BASE_PATH = '/v1/Organizations';

export const organizationsService = {
  list: async (pageSize: number = 50, cursor?: string): Promise<PagedResponse<Organization>> => {
    const params = new URLSearchParams();
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Organization> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string; description?: string }): Promise<Organization> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: { name?: string; description?: string }): Promise<Organization> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },

  search: async (name: string): Promise<Organization[]> => {
    return apiFetch(`${BASE_PATH}/search?name=${encodeURIComponent(name)}`);
  },
};
