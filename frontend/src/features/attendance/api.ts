// The attendance endpoints: sessions (one gathering), their types, and who came to which. No React in this file.
import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

export type PersonKind = "Member" | "Visitor";

// One time a person was checked in to a session.
export type AttendanceRecord = {
  id: string;
  sessionId: string;
  sessionDate: string;
  sessionName?: string | null;
  sessionLabelName: string;
  checkedInAt: string;
};

// A check-in as the session page sees it: the record plus who it is about. `personKind` is what they were when
// they were checked in (a visitor who became a member later still shows as a visitor here).
export type CheckIn = AttendanceRecord & {
  personId: string;
  personName: string;
  personKind: PersonKind;
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

// What one session's check-ins can be narrowed by: the name, and whether they were a member or a visitor.
export type CheckInFilters = { name?: string; kind?: PersonKind };

// Newest check-in first. A request for one session has no default range.
export const listCheckIns = (
  sessionId: string,
  filters: CheckInFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<CheckIn>>("/v1/attendance", {
    query: { sessionId, ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// What the records can narrow check-ins by. Both days are always sent, because a request with no person and no
// session has a default range of its own.
export type RecordsQuery = {
  name?: string;
  kind?: PersonKind;
  sessionLabelId?: number;
  from: string;
  to: string;
};

// Check-ins at every session in a range of days, the latest session first.
export const listCheckInsBetween = (
  query: RecordsQuery,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<CheckIn>>("/v1/attendance", {
    query: { ...query, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// The numbers for a session (by `sessionId`) or a range of days. A visitor counts as first-time when they
// have no earlier visit at all.
export type AttendanceSummary = {
  totalPresent: number;
  membersPresent: number;
  visitorsPresent: number;
  firstTimeVisitors: number;
};
export type SummaryScope = { sessionId: string } | { from: string; to: string };

export const getAttendanceSummary = (
  scope: SummaryScope,
  signal?: AbortSignal,
) =>
  api.get<AttendanceSummary>("/v1/attendance/summary", {
    query: { ...scope },
    signal,
  });

// Marks a person present. The time is left out, so the server uses the moment of the request. The server
// answers 409 when they are already checked in.
export const createCheckIn = (sessionId: string, personId: string) =>
  api.post<CheckIn>("/v1/attendance", { sessionId, personId });

// Only the time can change. The server cannot clear it.
export const updateCheckIn = (id: string, checkedInAt: string) =>
  api.put<CheckIn>(`/v1/attendance/${id}`, { checkedInAt });

export const deleteCheckIn = (id: string) =>
  api.delete<void>(`/v1/attendance/${id}`);

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
