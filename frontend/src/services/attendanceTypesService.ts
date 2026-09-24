import { apiFetch } from './api';
import type { AttendanceType } from '../types/attendance';

const BASE_PATH = '/v1/AttendanceTypes';

export const attendanceTypesService = {
  getAll: async (): Promise<AttendanceType[]> => {
    return apiFetch(BASE_PATH);
  },

  getById: async (id: number): Promise<AttendanceType> => {
    return apiFetch(`${BASE_PATH}/${id}`);
  },

  create: async (data: { name: string }): Promise<AttendanceType> => {
    return apiFetch(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  update: async (id: number, data: { name: string }): Promise<AttendanceType> => {
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
