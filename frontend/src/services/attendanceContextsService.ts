import type {
  AttendanceContextUpdatePayload,
  AttendanceContext,
  AttendanceContextCreatePayload,
  AttendanceContextSearchResult,
} from "../types/attendanceContext";
import { apiFetch } from "./api";

const BASE_PATH = "/v1/AttendanceContexts";

export const attendanceContextsService = {
  getAll: async (): Promise<AttendanceContext[]> => {
    return apiFetch<AttendanceContext[]>(BASE_PATH);
  },

  getById: async (id: number): Promise<AttendanceContext> => {
    return apiFetch<AttendanceContext>(`${BASE_PATH}/${id}`);
  },

  create: async (
    data: AttendanceContextCreatePayload,
  ): Promise<AttendanceContext> => {
    return apiFetch<AttendanceContext>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: number,
    data: AttendanceContextUpdatePayload,
  ): Promise<AttendanceContext> => {
    return apiFetch<AttendanceContext>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: number): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (name: string): Promise<AttendanceContextSearchResult[]> => {
    return apiFetch<AttendanceContextSearchResult[]>(
      `${BASE_PATH}/search?name=${encodeURIComponent(name)}`,
    );
  },
};
