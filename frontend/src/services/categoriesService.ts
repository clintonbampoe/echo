import { apiFetch } from './api';
import { Category } from '../types/category';

export type CategoryType = 'Asset' | 'Project' | 'Transaction';

const getBasePath = (type: CategoryType) => `/v1/${type}Categories`;

export const categoriesService = {
  list: async (type: CategoryType): Promise<Category[]> => {
    return apiFetch(getBasePath(type));
  },

  getById: async (type: CategoryType, id: number): Promise<Category> => {
    return apiFetch(`${getBasePath(type)}/${id}`);
  },

  create: async (type: CategoryType, data: Partial<Category>): Promise<Category> => {
    return apiFetch(getBasePath(type), {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (type: CategoryType, id: number, data: Partial<Category>): Promise<Category> => {
    return apiFetch(`${getBasePath(type)}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: async (type: CategoryType, id: number): Promise<void> => {
    return apiFetch(`${getBasePath(type)}/${id}`, {
      method: 'DELETE',
    });
  },
};
