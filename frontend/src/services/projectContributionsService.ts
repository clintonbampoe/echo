import { apiFetch } from './api';
import type { ProjectContribution, PagedResponse, ProjectContributionFilters } from '../types/project';

const BASE_PATH = '/v1/ProjectContributions';

function cleanContributionPayload(data: Partial<ProjectContribution>) {
  const payload: Record<string, any> = { ...data };

  delete payload.id;
  delete payload.projectName;
  delete payload.createdAt;

  if (payload.amount) {
    payload.amount = Number(payload.amount);
  }
  if (!payload.description || payload.description.trim() === '') {
    delete payload.description;
  }

  return payload;
}

export const projectContributionsService = {
  list: async (
    filters: ProjectContributionFilters = {},
    pageSize: number = 24,
    cursor?: string
  ): Promise<PagedResponse<ProjectContribution>> => {
    const params = new URLSearchParams();
    if (filters.amount) params.append('Amount', filters.amount.toString());
    if (filters.date) params.append('Date', filters.date);
    if (filters.paymentMethod) params.append('PaymentMethod', filters.paymentMethod);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<ProjectContribution> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<ProjectContribution>): Promise<ProjectContribution> => {
    const payload = cleanContributionPayload(data);
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<ProjectContribution>): Promise<ProjectContribution> => {
    const payload = cleanContributionPayload(data);
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
