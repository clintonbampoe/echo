// The only file in attendance that knows about TanStack Query.
import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const attendanceKeys = {
  person: (id: string, filters: api.AttendanceFilters) =>
    ["attendance", "person", id, filters] as const,
  count: (id: string, from: string, to: string) =>
    ["attendance", "count", id, from, to] as const,
  labels: ["attendance", "labels"] as const,
  recent: (label: string, today: string, size: number) =>
    ["attendance", "recent", label, today, size] as const,
  held: (from: string, to: string) => ["attendance", "held", from, to] as const,
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
