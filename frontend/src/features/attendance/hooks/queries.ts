import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import type {
  AttendanceFilters,
  CheckInFilters,
  RecordsQuery,
  SummaryScope,
} from "../api/checkIns";
import {
  NO_FILTERS,
  getAttendanceCount,
  getAttendanceSummary,
  listAttendance,
  listCheckIns,
  listCheckInsBetween,
} from "../api/checkIns";
import { listSessionLabels } from "../api/sessionLabels";
import type { SessionFilters } from "../api/sessions";
import {
  RECENT,
  getSession,
  getSessionCount,
  listRecentSessions,
  listSessions,
} from "../api/sessions";
import { attendanceKeys } from "./keys";

// Everything one person was checked in to, newest first.
export const useAttendance = (
  personId: string,
  filters: AttendanceFilters = NO_FILTERS,
) =>
  usePagedList(attendanceKeys.person(personId, filters), (c, s) =>
    listAttendance(personId, filters, c, s),
  );

export const useAttendanceCount = (
  personId: string,
  from: string,
  to: string,
) =>
  useQuery({
    queryKey: attendanceKeys.count(personId, from, to),
    queryFn: ({ signal }) => getAttendanceCount(personId, from, to, signal),
  });

// The kinds of session, for a Type filter. They hardly ever change, so they are kept for a while.
export const useSessionLabels = () =>
  useQuery({
    queryKey: attendanceKeys.labels,
    queryFn: ({ signal }) => listSessionLabels(signal),
    staleTime: 5 * 60_000,
  });

export const useRecentSessions = (
  sessionLabelId: string,
  today: string,
  size: number = RECENT,
) =>
  useQuery({
    queryKey: attendanceKeys.recent(sessionLabelId, today, size),
    queryFn: ({ signal }) =>
      listRecentSessions(sessionLabelId, today, size, signal),
  });

export const useHeldCount = (from: string, to: string) =>
  useQuery({
    queryKey: attendanceKeys.held(from, to),
    queryFn: ({ signal }) => getSessionCount(from, to, signal),
  });

// The sessions held (or planned) between two days, newest first. Only the first page: a church does not hold
// fifty gatherings in a fortnight. The last answer stays (as `isPlaceholderData`) while the next one loads, so the
// dots on the strip do not blink as the picked day moves.
export const useSessionsBetween = (from: string, to: string) =>
  useQuery({
    queryKey: attendanceKeys.between(from, to),
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) =>
      (await listSessions({ from, to }, undefined, signal)).data,
  });

export const useSession = (id: string) =>
  useQuery({
    queryKey: attendanceKeys.session(id),
    queryFn: ({ signal }) => getSession(id, signal),
  });

// The latest completed session of one type before a day: the one "came last time" is read from.
export const useSessionBefore = (sessionLabelId: number, day: string) =>
  useQuery({
    queryKey: attendanceKeys.before(sessionLabelId, day),
    queryFn: async ({ signal }) => {
      const page = await listSessions(
        { sessionLabelId, status: "Completed", from: "2000-01-01", to: day },
        undefined,
        signal,
      );
      return page.data.find((s) => s.date < day) ?? null;
    },
  });

// Who was checked in to one session, newest first.
export const useCheckIns = (sessionId: string, filters: CheckInFilters = {}) =>
  usePagedList(attendanceKeys.checkIns(sessionId, filters), (c, s) =>
    listCheckIns(sessionId, filters, c, s),
  );

// The sessions in a range of days, page after page, for the records.
export const useRecordSessions = (filters: SessionFilters) =>
  usePagedList(attendanceKeys.recordSessions(filters), (c, s) =>
    listSessions(filters, c, s),
  );

// Every check-in in a range of days, page after page, for the records.
export const useRecordCheckIns = (query: RecordsQuery) =>
  usePagedList(attendanceKeys.recordCheckIns(query), (c, s) =>
    listCheckInsBetween(query, c, s),
  );

export const useAttendanceSummary = (scope: SummaryScope) =>
  useQuery({
    queryKey: attendanceKeys.summary(scope),
    queryFn: ({ signal }) => getAttendanceSummary(scope, signal),
  });
