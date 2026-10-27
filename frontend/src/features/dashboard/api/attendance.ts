import { api } from "@/lib/api";

export type AttendanceSummaryChartSummary = {
  totalPresent: number;
  membersPresent: number;
  visitorsPresent: number;
  firstTimeVisitors: number;
};

export type AttendanceSummaryChartBucket = {
  month: string; // "2026-05-01"
  totalPresent: number;
  membersPresent: number;
  visitorsPresent: number;
  firstTimeVisitors: number;
};

export type AttendanceSummaryChartResponse = {
  months: 3 | 6 | 12;
  buckets: AttendanceSummaryChartBucket[];
  summary: AttendanceSummaryChartSummary;
};

export const getAttendanceSummaryChart = (months: 3 | 6 | 12, signal?: AbortSignal) =>
  api.get<AttendanceSummaryChartResponse>("/v1/attendance/summary/chart", {
    query: { months },
    signal,
  });
