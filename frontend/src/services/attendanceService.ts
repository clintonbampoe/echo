import { apiFetch } from './api';
import type { AttendanceRecord, PagedResponse, AttendanceFilters } from '../types/attendance';

const BASE_PATH = '/v1/Attendance';

function cleanAttendancePayload(data: Partial<AttendanceRecord>) {
  const payload: Record<string, any> = { ...data };

  delete payload.id;
  delete payload.attendanceContextName;
  delete payload.attendanceTypeName;
  delete payload.memberName;
  delete payload.createdAt;

  if (payload.attendanceContextId) {
    payload.attendanceContextId = Number(payload.attendanceContextId);
  }

  if (!payload.description || payload.description.trim() === '') {
    delete payload.description;
  }

  return payload;
}

export const attendanceService = {
  list: async (
    filters: AttendanceFilters = {},
    pageSize: number = 50,
    cursor?: string
  ): Promise<PagedResponse<AttendanceRecord>> => {
    const params = new URLSearchParams();
    if (filters.forDate) params.append('ForDate', filters.forDate);
    if (filters.attendanceContextId) params.append('AttendanceContextId', filters.attendanceContextId.toString());
    if (filters.memberId) params.append('MemberId', filters.memberId);
    if (filters.memberName) params.append('MemberName', filters.memberName);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);

    return apiFetch(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<AttendanceRecord> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<AttendanceRecord>): Promise<AttendanceRecord> => {
    const payload = cleanAttendancePayload(data);
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<AttendanceRecord>): Promise<AttendanceRecord> => {
    const payload = cleanAttendancePayload(data);
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
