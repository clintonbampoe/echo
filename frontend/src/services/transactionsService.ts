import { apiFetch } from './api';
import type { Transaction, PagedResponse, TransactionFilters } from '../types/finance';

const BASE_PATH = '/v1/Transactions';

function cleanTransactionPayload(data: Partial<Transaction>) {
  const payload: Record<string, unknown> = { ...data };

  delete payload.id;
  delete payload.categoryName;
  delete payload.createdAt;

  if (payload.categoryId) {
    payload.categoryId = Number(payload.categoryId);
  }

  if (payload.amount) {
    payload.amount = Number(payload.amount);
  }

  if (payload.transactionDate && typeof payload.transactionDate === 'string') {
    payload.transactionDate = payload.transactionDate.split('T')[0];
  }

  if (!payload.description || (typeof payload.description === 'string' && payload.description.trim() === '')) {
    delete payload.description;
  }

  return payload;
}

export const transactionsService = {
  list: async (
    filters: TransactionFilters = {},
    pageSize: number = 50,
    cursor?: string
  ): Promise<PagedResponse<Transaction>> => {
    const params = new URLSearchParams();
    if (filters.date) params.append('Date', filters.date);
    if (filters.transactionType) params.append('TransactionType', filters.transactionType);
    if (filters.categoryId) params.append('CategoryId', filters.categoryId.toString());
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch<PagedResponse<Transaction>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Transaction> => {
    return apiFetch<Transaction>(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Transaction>): Promise<Transaction> => {
    const payload = cleanTransactionPayload(data);
    return apiFetch<Transaction>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Transaction>): Promise<Transaction> => {
    const payload = cleanTransactionPayload(data);
    return apiFetch<Transaction>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },
};
