import { apiFetch } from './api';
import type { TransactionCategory, TransactionType } from '../types/finance';

const BASE_PATH = '/v1/TransactionCategories';

export const transactionCategoriesService = {
  list: async (): Promise<TransactionCategory[]> => {
    return apiFetch(BASE_PATH);
  },

  getById: async (id: number): Promise<TransactionCategory> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string; categoryType: TransactionType }): Promise<TransactionCategory> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: number, data: { name?: string; categoryType?: TransactionType }): Promise<TransactionCategory> => {
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

  search: async (q: string): Promise<TransactionCategory[]> => {
    return apiFetch(`${BASE_PATH}/search?q=${encodeURIComponent(q)}`);
  },
};
