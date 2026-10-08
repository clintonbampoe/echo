import { apiFetch } from "./api";
import type {
  Event,
  EventCreatePayload,
  EventUpdatePayload,
  EventFilters,
  EventSearchResult,
} from "../types/event";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/events";

export const eventsService = {
  list: async (
    filters: EventFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<Event>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append("Name", filters.name);
    if (filters.organizationId)
      params.append("OrganizationId", filters.organizationId);
    if (filters.organizerId) params.append("OrganizerId", filters.organizerId);
    if (filters.startDate) params.append("StartDate", filters.startDate);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch<PagedResponse<Event>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Event> => {
    return apiFetch<Event>(`${BASE_PATH}/${id}`);
  },

  create: async (data: EventCreatePayload): Promise<Event> => {
    return apiFetch<Event>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: EventUpdatePayload): Promise<Event> => {
    return apiFetch<Event>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (name: string): Promise<EventSearchResult[]> => {
    return apiFetch<EventSearchResult[]>(
      `${BASE_PATH}/search?name=${encodeURIComponent(name)}`,
    );
  },
};
