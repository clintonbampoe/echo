import { apiFetch } from './api';
import type { Project, PagedResponse, ProjectFilters } from '../types/project';

const BASE_PATH = '/v1/Projects';

function cleanProjectPayload(data: Partial<Project>) {
  const payload: Record<string, unknown> = { ...data };

  delete payload.id;
  delete payload.categoryName;
  delete payload.managerName;
  delete payload.createdAt;
  delete payload.raisedAmount;

  if (payload.targetAmount) {
    payload.targetAmount = Number(payload.targetAmount);
  }
  if (payload.categoryId) {
    payload.categoryId = Number(payload.categoryId);
  }

  if (!payload.endDate || (typeof payload.endDate === 'string' && payload.endDate.trim() === '')) delete payload.endDate;
  if (!payload.description || (typeof payload.description === 'string' && payload.description.trim() === '')) delete payload.description;

  return payload;
}

export const projectsService = {
  list: async (filters: ProjectFilters = {}, pageSize: number = 24, cursor?: string): Promise<PagedResponse<Project>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append('Name', filters.name);
    if (filters.status) params.append('Status', filters.status);
    if (filters.categoryId) params.append('CategoryId', filters.categoryId.toString());
    if (filters.startDate) params.append('StartDate', filters.startDate);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch<PagedResponse<Project>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Project> => {
    return apiFetch<Project>(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Project>): Promise<Project> => {
    const payload = cleanProjectPayload(data);
    return apiFetch<Project>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Project>): Promise<Project> => {
    const payload = cleanProjectPayload(data);
    return apiFetch<Project>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },

  search: async (query: string): Promise<Array<{ id: string; name: string }>> => {
    return apiFetch<Array<{ id: string; name: string }>>(`${BASE_PATH}/search?q=${encodeURIComponent(query)}`);
  },
};
