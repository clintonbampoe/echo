import { api } from "@/lib/api";

// A kind of session, like "Sunday Service", and the category that groups it in reports. The API sends them all as one plain list.
export type SessionCategory =
  | "Service"
  | "Meeting"
  | "Study"
  | "Prayer"
  | "Practice"
  | "Outreach"
  | "Class"
  | "Special";

export type SessionLabel = {
  id: number;
  name: string;
  category: SessionCategory;
};

export const listSessionLabels = (signal?: AbortSignal) =>
  api.get<SessionLabel[]>("/v1/attendance/labels", { signal });

// What the API accepts for a type. The category is chosen once: an update can only rename.
export type SessionLabelInput = { name: string; category: SessionCategory };

export const createSessionLabel = (input: SessionLabelInput) =>
  api.post<SessionLabel>("/v1/attendance/labels", input);

export const updateSessionLabel = (id: number, name: string) =>
  api.put<SessionLabel>(`/v1/attendance/labels/${id}`, { name });

export const deleteSessionLabel = (id: number) =>
  api.delete<void>(`/v1/attendance/labels/${id}`);
