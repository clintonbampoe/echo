import { api } from "@/lib/api";

// An event as a dashboard card needs it. Days are plain "2026-10-24"; endDate is the last day, the same as
// startDate for a single day.
export type DashEvent = {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  startTime?: string | null;
  registrationCount: number;
};

// Events that start in the range (by the day they start), in date order. The server answers at most 50.
export const getEvents = (
  from: string,
  to: string,
  order: "Asc" | "Desc",
  pageSize: number,
  signal?: AbortSignal,
) =>
  api
    .get<{ data: DashEvent[] }>("/v1/events", {
      query: { from, to, order, pageSize },
      signal,
    })
    .then((r) =>
      r.data.map((e) => ({
        ...e,
        startDate: e.startDate.slice(0, 10),
        endDate: e.endDate.slice(0, 10),
      })),
    );
