import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";

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
    query: { personId, ...filters, cursor, pageSize: pageSize() },
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
    query: { sessionId, ...filters, cursor, pageSize: pageSize() },
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
    query: { ...query, cursor, pageSize: pageSize() },
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
