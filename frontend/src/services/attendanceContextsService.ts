import { apiFetch } from './api';
import type { AttendanceContext } from '../types/attendance';

const BASE_PATH = '/v1/AttendanceContexts';

export const attendanceContextsService = {
  getAll: async (): Promise<AttendanceContext[]> => {
    return apiFetch(BASE_PATH);
  },

  getById: async (id: number): Promise<AttendanceContext> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string; attendanceTypeId: number }): Promise<AttendanceContext> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: number, data: { name: string }): Promise<AttendanceContext> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  delete: async (id: number): Promise<void> => {
    return apiFetch(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },
};
