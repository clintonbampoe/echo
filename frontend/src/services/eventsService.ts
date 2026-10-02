import { apiFetch } from "./api";
import type {
  Event,
  PagedResponse,
  EventFilters,
  EventCreatePayload,
  EventUpdatePayload,
} from "../types/event";

const BASE_PATH = "/v1/Events";

export const eventsService = {
  list: async (
    filters: EventFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<Event>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append("Name", filters.name);
    if (filters.organizationId)
      params.append("OrganizationId", filters.organizationId.toString());
    if (filters.startDate) params.append("StartDate", filters.startDate);
    if (filters.endDate) params.append("EndDate", filters.endDate);
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
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: "DELETE",
    });
  },

  search: async (name: string): Promise<Event[]> => {
    return apiFetch<Event[]>(
      `${BASE_PATH}/search?name=${encodeURIComponent(name)}`,
    );
  },
};
