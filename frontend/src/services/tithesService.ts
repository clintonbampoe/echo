import { apiFetch } from './api';
import type { Tithe, PagedResponse, TitheFilters } from '../types/finance';

const BASE_PATH = '/v1/Tithes';

function cleanTithePayload(data: Partial<Tithe>) {
  const payload: Record<string, any> = { ...data };

  delete payload.id;
  delete payload.memberName;
  delete payload.createdAt;

  if (payload.amount) {
    payload.amount = Number(payload.amount);
  }

  if (payload.forYear) {
    payload.forYear = Number(payload.forYear);
  }

  if (payload.collectionDate && typeof payload.collectionDate === 'string') {
    payload.collectionDate = payload.collectionDate.split('T')[0];
  }

  if (!payload.description || payload.description.trim() === '') {
    delete payload.description;
  }

  return payload;
}

export const tithesService = {
  list: async (
    filters: TitheFilters = {},
    pageSize: number = 50,
    cursor?: string
  ): Promise<PagedResponse<Tithe>> => {
    const params = new URLSearchParams();
    if (filters.year) params.append('Year', filters.year.toString());
    if (filters.month) params.append('Month', filters.month);
    if (filters.paymentMethod) params.append('PaymentMethod', filters.paymentMethod);
    if (filters.memberId) params.append('MemberId', filters.memberId);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Tithe> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Tithe>): Promise<Tithe> => {
    const payload = cleanTithePayload(data);
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Tithe>): Promise<Tithe> => {
    const payload = cleanTithePayload(data);
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },
};
