import { apiFetch } from "./api";
import type {
  AttendanceType,
  AttendanceTypeCreatePayload,
  AttendanceTypeUpdatePayload,
  AttendanceTypeSearchResult,
} from "../types/attendanceType";

const BASE_PATH = "/v1/attendance-types";

export const attendanceTypesService = {
  getAll: async (): Promise<AttendanceType[]> => {
    return apiFetch<AttendanceType[]>(BASE_PATH);
  },

  getById: async (id: number): Promise<AttendanceType> => {
    return apiFetch<AttendanceType>(`${BASE_PATH}/${id}`);
  },

  create: async (
    data: AttendanceTypeCreatePayload,
  ): Promise<AttendanceType> => {
    return apiFetch<AttendanceType>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: number,
    data: AttendanceTypeUpdatePayload,
  ): Promise<AttendanceType> => {
    return apiFetch<AttendanceType>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: number): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (name: string): Promise<AttendanceTypeSearchResult[]> => {
    return apiFetch<AttendanceTypeSearchResult[]>(
      `${BASE_PATH}/search?name=${encodeURIComponent(name)}`,
    );
  },
};
