import { apiFetch } from './api';
import type { ProjectCategory } from '../types/project';

const BASE_PATH = '/v1/ProjectCategories';

export const projectCategoriesService = {
  getAll: async (): Promise<ProjectCategory[]> => {
    const res = await apiFetch(BASE_PATH);
    return Array.isArray(res) ? res : res?.data || [];
  },

  getById: async (id: number): Promise<ProjectCategory> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string }): Promise<ProjectCategory> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: number, data: { name: string }): Promise<ProjectCategory> => {
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
};
