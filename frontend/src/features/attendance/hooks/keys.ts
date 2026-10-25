import type {
  AttendanceFilters,
  CheckInFilters,
  RecordsQuery,
  SummaryScope,
} from "../api/checkIns";
import type { SessionFilters } from "../api/sessions";

export const attendanceKeys = {
  all: ["attendance"] as const,
  person: (id: string, filters: AttendanceFilters) =>
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
  checkIns: (sessionId: string, filters: CheckInFilters) =>
    ["attendance", "check-ins", sessionId, filters] as const,
  recordSessions: (filters: SessionFilters) =>
    ["attendance", "records", "sessions", filters] as const,
  recordCheckIns: (query: RecordsQuery) =>
    ["attendance", "records", "check-ins", query] as const,
  summary: (scope: SummaryScope) => ["attendance", "summary", scope] as const,
};
