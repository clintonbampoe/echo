// The only file in attendance that knows about TanStack Query.
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const attendanceKeys = {
  all: ["attendance"] as const,
  person: (id: string, filters: api.AttendanceFilters) =>
    ["attendance", "person", id, filters] as const,
  count: (id: string, from: string, to: string) =>
    ["attendance", "count", id, from, to] as const,
  labels: ["attendance", "labels"] as const,
  recent: (label: string, today: string, size: number) =>
    ["attendance", "recent", label, today, size] as const,
  held: (from: string, to: string) => ["attendance", "held", from, to] as const,
  between: (from: string, to: string) =>
    ["attendance", "sessions", "between", from, to] as const,
  session: (id: string) => ["attendance", "sessions", "one", id] as const,
  before: (label: number, day: string) =>
    ["attendance", "sessions", "before", label, day] as const,
  checkIns: (sessionId: string, filters: api.CheckInFilters) =>
    ["attendance", "check-ins", sessionId, filters] as const,
  recordSessions: (filters: api.SessionFilters) =>
    ["attendance", "records", "sessions", filters] as const,
  recordCheckIns: (query: api.RecordsQuery) =>
    ["attendance", "records", "check-ins", query] as const,
  summary: (scope: api.SummaryScope) =>
    ["attendance", "summary", scope] as const,
};

// Everything one person was checked in to, newest first.
export const useAttendance = (
  personId: string,
  filters: api.AttendanceFilters = api.NO_FILTERS,
) =>
  usePagedList(attendanceKeys.person(personId, filters), (c, s) =>
    api.listAttendance(personId, filters, c, s),
  );

export const useAttendanceCount = (
  personId: string,
  from: string,
  to: string,
) =>
  useQuery({
    queryKey: attendanceKeys.count(personId, from, to),
    queryFn: ({ signal }) => api.getAttendanceCount(personId, from, to, signal),
  });

// The kinds of session, for a Type filter. They hardly ever change, so they are kept for a while.
export const useSessionLabels = () =>
  useQuery({
    queryKey: attendanceKeys.labels,
    queryFn: ({ signal }) => api.listSessionLabels(signal),
    staleTime: 5 * 60_000,
  });

export const useRecentSessions = (
  sessionLabelId: string,
  today: string,
  size: number = api.RECENT,
) =>
  useQuery({
    queryKey: attendanceKeys.recent(sessionLabelId, today, size),
    queryFn: ({ signal }) =>
      api.listRecentSessions(sessionLabelId, today, size, signal),
  });

export const useHeldCount = (from: string, to: string) =>
  useQuery({
    queryKey: attendanceKeys.held(from, to),
    queryFn: ({ signal }) => api.getSessionCount(from, to, signal),
  });

// The sessions held (or planned) between two days, newest first. Only the first page: a church does not hold
// fifty gatherings in a fortnight. The last answer stays (as `isPlaceholderData`) while the next one loads, so the
// dots on the strip do not blink as the picked day moves.
export const useSessionsBetween = (from: string, to: string) =>
  useQuery({
    queryKey: attendanceKeys.between(from, to),
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) =>
      (await api.listSessions({ from, to }, undefined, signal)).data,
  });

export const useSession = (id: string) =>
  useQuery({
    queryKey: attendanceKeys.session(id),
    queryFn: ({ signal }) => api.getSession(id, signal),
  });

// The latest completed session of one type before a day: the one "came last time" is read from.
export const useSessionBefore = (sessionLabelId: number, day: string) =>
  useQuery({
    queryKey: attendanceKeys.before(sessionLabelId, day),
    queryFn: async ({ signal }) => {
      const page = await api.listSessions(
        { sessionLabelId, status: "Completed", from: "2000-01-01", to: day },
        undefined,
        signal,
      );
      return page.data.find((s) => s.date < day) ?? null;
    },
  });

// Who was checked in to one session, newest first.
export const useCheckIns = (
  sessionId: string,
  filters: api.CheckInFilters = {},
) =>
  usePagedList(attendanceKeys.checkIns(sessionId, filters), (c, s) =>
    api.listCheckIns(sessionId, filters, c, s),
  );

// The sessions in a range of days, page after page, for the records.
export const useRecordSessions = (filters: api.SessionFilters) =>
  usePagedList(attendanceKeys.recordSessions(filters), (c, s) =>
    api.listSessions(filters, c, s),
  );

// Every check-in in a range of days, page after page, for the records.
export const useRecordCheckIns = (query: api.RecordsQuery) =>
  usePagedList(attendanceKeys.recordCheckIns(query), (c, s) =>
    api.listCheckInsBetween(query, c, s),
  );

export const useAttendanceSummary = (scope: api.SummaryScope) =>
  useQuery({
    queryKey: attendanceKeys.summary(scope),
    queryFn: ({ signal }) => api.getAttendanceSummary(scope, signal),
  });

// Any change to a session, a type or a check-in changes the lists and numbers on several screens.
function useRefresh() {
  const client = useQueryClient();
  // Not awaited: the form that saved moves on at once, and the lists refresh behind it.
  return () => void client.invalidateQueries({ queryKey: attendanceKeys.all });
}

export function useCreateSession() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.createSession, onSuccess: refresh });
}

export function useUpdateSession() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & api.SessionUpdate) =>
      api.updateSession(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteSession() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.deleteSession, onSuccess: refresh });
}

export function useCheckIn(sessionId: string) {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: (personId: string) => api.createCheckIn(sessionId, personId),
    onSuccess: refresh,
  });
}

export function useUpdateCheckIn() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, checkedInAt }: { id: string; checkedInAt: string }) =>
      api.updateCheckIn(id, checkedInAt),
    onSuccess: refresh,
  });
}

export function useRemoveCheckIn() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.deleteCheckIn, onSuccess: refresh });
}

export function useCreateSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: api.createSessionLabel,
    onSuccess: refresh,
  });
}

export function useUpdateSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      api.updateSessionLabel(id, name),
    onSuccess: refresh,
  });
}

export function useDeleteSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: api.deleteSessionLabel,
    onSuccess: refresh,
  });
}
