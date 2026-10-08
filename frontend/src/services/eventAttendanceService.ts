import { apiFetch } from "./api";
import type {
  EventAttendance,
  EventAttendanceCreatePayload,
  EventAttendanceUpdatePayload,
} from "../types/eventAttendance";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/eventattendance";

export const eventAttendanceService = {
  list: async (
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<EventAttendance>> => {
    const params = new URLSearchParams();
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  listByEventId: async (
    eventId: string,
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<EventAttendance>> => {
    const params = new URLSearchParams();
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch(`${BASE_PATH}/event${eventId}?${params.toString()}`);
  },

  listByMemberId: async (
    memberId: string,
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<EventAttendance>> => {
    const params = new URLSearchParams();
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch(`${BASE_PATH}/member${memberId}?${params.toString()}`);
  },

  getById: async (id: string): Promise<EventAttendance> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (
    data: EventAttendanceCreatePayload,
  ): Promise<EventAttendance> => {
    return apiFetch(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: EventAttendanceUpdatePayload,
  ): Promise<EventAttendance> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
