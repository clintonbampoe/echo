// The attendance endpoints: who came to which session. No React in this file.
import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

// One time a person was checked in to a session.
export type AttendanceRecord = {
  id: string;
  sessionId: string;
  sessionDate: string;
  sessionName?: string | null;
  sessionLabelName: string;
  checkedInAt?: string | null;
};

// What a person's attendance can be narrowed by: the kind of session (a label's id) and a range of days.
export type AttendanceFilters = {
  sessionLabelId?: string;
  from?: string;
  to?: string;
};
export const NO_FILTERS: AttendanceFilters = {};

export const listAttendance = (
  personId: string,
  filters: AttendanceFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<AttendanceRecord>>("/v1/attendance", {
    query: { personId, ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// A kind of session, like "Sunday Service". The API sends them all as one plain list.
export type SessionLabel = { id: number; name: string };

export const listSessionLabels = (signal?: AbortSignal) =>
  api.get<SessionLabel[]>("/v1/attendance/labels", { signal });

// A session that was held.
export type HeldSession = {
  id: string;
  date: string;
  name?: string | null;
  sessionLabelName: string;
};

// How many of the latest sessions the strip shows.
export const RECENT = 16;

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

// How many sessions the person attended in a period (from and to are plain "2026-01-01" days).
export const getAttendanceCount = (
  personId: string,
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  api.get<{ totalPresent: number }>("/v1/attendance/summary", {
    query: { personId, from, to },
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
