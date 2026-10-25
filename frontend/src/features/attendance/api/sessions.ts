import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";
import type { SessionCategory } from "./sessionLabels";

export type SessionStatus = "Scheduled" | "Completed" | "Cancelled";

// One gathering: a Sunday service, a Wednesday Bible study. `startsAt` and `endsAt` are times of day ("08:00:00").
export type Session = {
  id: string;
  sessionLabelId: number;
  sessionLabelName: string;
  category: SessionCategory;
  date: string;
  name?: string | null;
  topic?: string | null;
  notes?: string | null;
  startsAt?: string | null;
  endsAt?: string | null;
  status: SessionStatus;
  createdByName?: string | null;
  createdAt: string;
  attendanceCount: number;
};

// What the day view and the history can narrow sessions by. With no days the server answers for this week only.
export type SessionFilters = {
  sessionLabelId?: number;
  status?: SessionStatus;
  name?: string;
  from?: string;
  to?: string;
};

export const listSessions = (
  filters: SessionFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Session>>("/v1/attendance/sessions", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

export const getSession = (id: string, signal?: AbortSignal) =>
  api.get<Session>(`/v1/attendance/sessions/${id}`, { signal });

// What the API accepts when creating a session.
export type SessionInput = {
  sessionLabelId: number;
  date: string;
  name?: string;
  topic?: string;
  notes?: string;
  startsAt?: string;
  endsAt?: string;
};

// An update cannot move the session to another type or day, and cannot take a time away (only change it).
// Text is cleared by sending it empty.
export type SessionUpdate = {
  name?: string;
  topic?: string;
  notes?: string;
  startsAt?: string;
  endsAt?: string;
  status?: SessionStatus;
};

export const createSession = (input: SessionInput) =>
  api.post<Session>("/v1/attendance/sessions", input);

export const updateSession = (id: string, input: SessionUpdate) =>
  api.put<Session>(`/v1/attendance/sessions/${id}`, input);

export const deleteSession = (id: string) =>
  api.delete<void>(`/v1/attendance/sessions/${id}`);

// A session that was held.
export type HeldSession = {
  id: string;
  date: string;
  name?: string | null;
  sessionLabelName: string;
};

// How many of the latest sessions the strip shows.
export const RECENT = 12;

// Sessions have no "everything" mode either: with no dates the server answers for this week only.
export const listRecentSessions = (
  sessionLabelId: string,
  today: string,
  size: number = RECENT,
  signal?: AbortSignal,
) =>
  api.get<Page<HeldSession>>("/v1/attendance/sessions", {
    query: {
      sessionLabelId,
      status: "Completed",
      from: "2000-01-01",
      to: today,
      pageSize: size,
    },
    signal,
  });

// How many sessions were held in the same period, so the two numbers can be set side by side.
export const getSessionCount = (
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  api.get<{ completedSessions: number }>("/v1/attendance/sessions/summary", {
    query: { from, to },
    signal,
  });
