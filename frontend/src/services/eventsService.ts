import { apiFetch } from './api';
import type { Event, PagedResponse, EventFilters } from '../types/event';

const BASE_PATH = '/v1/Events';

function cleanEventPayload(data: Partial<Event>) {
  const payload: Record<string, unknown> = { ...data };

  // Remove read-only or response properties
  delete payload.id;
  delete payload.organizationName;
  delete payload.createdAt;

  if (payload.organizationId) {
    payload.organizationId = Number(payload.organizationId);
  }

  // Format DateOnly and TimeOnly if present
  if (payload.eventDate && typeof payload.eventDate === 'string') {
    payload.eventDate = payload.eventDate.split('T')[0];
  }
  if (payload.eventEndDate && typeof payload.eventEndDate === 'string') {
    payload.eventEndDate = payload.eventEndDate.split('T')[0];
  }

  // Ensure empty strings are stripped
  if (!payload.description || (typeof payload.description === 'string' && payload.description.trim() === '')) {
    delete payload.description;
  }
  if (!payload.bannerUrl || (typeof payload.bannerUrl === 'string' && payload.bannerUrl.trim() === '')) {
    delete payload.bannerUrl;
  }

  return payload;
}

export const eventsService = {
  list: async (filters: EventFilters = {}, pageSize: number = 24, cursor?: string): Promise<PagedResponse<Event>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append('Name', filters.name);
    if (filters.organizationId) params.append('OrganizationId', filters.organizationId.toString());
    if (filters.startDate) params.append('StartDate', filters.startDate);
    if (filters.endDate) params.append('EndDate', filters.endDate);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch<PagedResponse<Event>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Event> => {
    return apiFetch<Event>(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Event>): Promise<Event> => {
    const payload = cleanEventPayload(data);
    return apiFetch<Event>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Event>): Promise<Event> => {
    const payload = cleanEventPayload(data);
    return apiFetch<Event>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },

  search: async (name: string): Promise<Event[]> => {
    return apiFetch<Event[]>(`${BASE_PATH}/search?name=${encodeURIComponent(name)}`);
  },
};
