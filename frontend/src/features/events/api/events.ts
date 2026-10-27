import { api, type Page } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";
import { pageSize } from "@/lib/paged";

export type OrganizedEvent = {
  id: string;
  name: string;
  organizationName: string;
  startDate: string;
  registrationCount: number;
};

// With no dates the server answers for this year only, so an organizer's list asks for all time.
// Enums bind case-sensitively: "Desc", not "desc".
export const listOrganizedEvents = (
  organizerId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<OrganizedEvent>>("/v1/events", {
    query: {
      organizerId,
      ...ALL_TIME,
      order: "Desc",
      cursor,
      pageSize: pageSize(),
    },
    signal,
  });

// An event as an organization's page lists it: when, where and who is running it.
export type HostedEvent = {
  id: string;
  name: string;
  startDate: string;
  startTime?: string | null;
  location?: string | null;
  organizerId: string;
  organizerName: string;
};

// The events an organization is hosting from `from` on, soonest first. With no dates the server answers
// for this year only, so the far end is wide. Enums bind case-sensitively: "Asc", not "asc".
export const listUpcomingEvents = (
  organizationId: string,
  from: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<HostedEvent>>("/v1/events", {
    query: {
      organizationId,
      from,
      to: ALL_TIME.to,
      order: "Asc",
      cursor,
      pageSize: pageSize(),
    },
    signal,
  });

// An event as the Events page lists it. endDate is the last day (the same as startDate for a single day).
export type EventItem = {
  id: string;
  name: string;
  organizationId: string;
  organizationName: string;
  organizerId: string;
  organizerName: string;
  startDate: string;
  endDate: string;
  startTime?: string | null;
  endTime?: string | null;
  location?: string | null;
  // The most people it takes. Not set means no limit.
  capacity?: number | null;
  description?: string | null;
  createdAt: string;
  registrationCount: number;
  // How many of the registered came. The list does not send it yet, so a row only shows it when it is there.
  attendanceCount?: number;
};

// What the Events list asks for. The range is by the day an event starts, and the server answers for this year
// only when it is not given, so it always is. Enums bind case-sensitively: "Asc" or "Desc".
export type EventQuery = {
  name?: string;
  organizationId?: string;
  organizerId?: string;
  from: string;
  to: string;
  order: "Asc" | "Desc";
};

const NONE: Page<never> = { data: [], hasMore: false, next: null };

// A range that ends before it starts (upcoming events of last year, say) holds nothing: no need to ask.
export const listEvents = (
  q: EventQuery,
  cursor?: string,
  signal?: AbortSignal,
  size: number = pageSize(),
): Promise<Page<EventItem>> =>
  q.from > q.to
    ? Promise.resolve(NONE)
    : api.get<Page<EventItem>>("/v1/events", {
        query: { ...q, cursor, pageSize: size },
        signal,
      });

// The numbers above the list. They count every event, whatever the day.
export type EventSummary = {
  totalEvents: number;
  upcomingEvents: number;
  totalRegistered: number;
  totalAttended: number;
};

export const getEventSummary = (
  organizationId: string | undefined,
  signal?: AbortSignal,
) =>
  api.get<EventSummary>("/v1/events/summary", {
    query: { ...ALL_TIME, organizationId },
    signal,
  });

// What the API accepts when adding an event, or changing one (an update may leave anything out). Times are
// "08:00:00". On an update, a time or the capacity that is left out stays as it was: they can be changed, not removed.
export type EventInput = {
  organizationId: string;
  organizerId: string;
  name: string;
  startDate: string;
  endDate: string;
  startTime?: string;
  endTime?: string;
  location?: string;
  capacity?: number;
  description?: string;
};

export const createEvent = (input: EventInput) =>
  api.post<EventItem>("/v1/events", input);

export const updateEvent = (id: string, input: Partial<EventInput>) =>
  api.put<EventItem>(`/v1/events/${id}`, input);

export const deleteEvent = (id: string) => api.delete<void>(`/v1/events/${id}`);

// One event, with the same fields as a row of the list.
export const getEvent = (id: string, signal?: AbortSignal) =>
  api.get<EventItem>(`/v1/events/${id}`, { signal });
