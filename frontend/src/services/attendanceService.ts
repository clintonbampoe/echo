import { apiFetch } from "./api";
import type {
  AttendanceRecord,
  AttendanceFilters,
  AttendanceCreatePayload,
  AttendanceUpdatePayload,
} from "../types/attendance";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/attendance";

export const attendanceService = {
  list: async (
    filters: AttendanceFilters = {},
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<AttendanceRecord>> => {
    const params = new URLSearchParams();
    if (filters.forDate) params.append("ForDate", filters.forDate);
    if (filters.attendanceContextId)
      params.append(
        "AttendanceContextId",
        filters.attendanceContextId.toString(),
      );
    if (filters.memberId) params.append("MemberId", filters.memberId);
    if (filters.memberName) params.append("MemberName", filters.memberName);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<AttendanceRecord>>(
      `${BASE_PATH}?${params.toString()}`,
    );
  },

  getById: async (id: string): Promise<AttendanceRecord> => {
    return apiFetch<AttendanceRecord>(`${BASE_PATH}/${id}`);
  },

  create: async (data: AttendanceCreatePayload): Promise<AttendanceRecord> => {
    return apiFetch<AttendanceRecord>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: AttendanceUpdatePayload,
  ): Promise<AttendanceRecord> => {
    return apiFetch<AttendanceRecord>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
