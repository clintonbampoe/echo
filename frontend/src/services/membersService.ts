import { apiFetch } from './api';
import type { Member, PagedResponse, MemberFilters } from '../types/member';

const BASE_PATH = '/v1/Members';

export const membersService = {
  list: async (filters: MemberFilters = {}, pageSize: number = 24, cursor?: string): Promise<PagedResponse<Member>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append('Name', filters.name);
    if (filters.status) params.append('Status', filters.status);
    if (filters.gender) params.append('Gender', filters.gender);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);
    
    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Member> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Member>): Promise<Member> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: Partial<Member>): Promise<Member> => {
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
};
