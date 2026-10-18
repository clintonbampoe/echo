// The event endpoints: events, who signed up and who came. No React in this file.
import { api, type Page } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";
import { PAGE_SIZE } from "@/lib/paged";

// A person's sign-up for an event. eventStartDate is the day it happens, registrationDate the day they signed up.
export type Registration = {
  id: string;
  eventId: string;
  eventName: string;
  eventStartDate: string;
  registrationDate: string;
};

export const listPersonRegistrations = (
  personId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Registration>>(`/v1/event-registrations/member/${personId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

// A check-in at an event's door. checkInTime is a time of day ("08:42:00"); the day is eventStartDate.
export type CheckIn = {
  id: string;
  eventId: string;
  eventName: string;
  eventStartDate: string;
  checkInTime: string;
};

export const listPersonCheckIns = (
  personId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<CheckIn>>(`/v1/event-attendance/member/${personId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

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
      pageSize: PAGE_SIZE,
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
      pageSize: PAGE_SIZE,
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
  pageSize = PAGE_SIZE,
): Promise<Page<EventItem>> =>
  q.from > q.to
    ? Promise.resolve(NONE)
    : api.get<Page<EventItem>>("/v1/events", {
        query: { ...q, cursor, pageSize },
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

// A member's sign-up for this event. registrationDate is the day they signed up.
export type EventRegistration = {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  registrationDate: string;
};

// A member's check-in at this event. checkInTime is a time of day ("08:42:00"); the day is the event's.
export type EventCheckIn = {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  checkInTime: string;
};

// Who signed up, in the order they did. The server has no search or filter here, so a page that needs them reads every page.
export const listEventRegistrations = (
  eventId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<EventRegistration>>(`/v1/event-registrations/event/${eventId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

// Who came, in the order they arrived.
export const listEventCheckIns = (
  eventId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<EventCheckIn>>(`/v1/event-attendance/event/${eventId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

export const deleteRegistration = (id: string) =>
  api.delete<void>(`/v1/event-registrations/${id}`);

export const deleteCheckIn = (id: string) =>
  api.delete<void>(`/v1/event-attendance/${id}`);

// The time can be changed, not taken away.
export const updateCheckInTime = (id: string, checkInTime: string) =>
  api.put<EventCheckIn>(`/v1/event-attendance/${id}`, { checkInTime });

// Signs a member up. registrationDate is the day they signed up. A full event answers 409 with the code EVENT_FULL.
export const registerMember = (input: {
  memberId: string;
  eventId: string;
  registrationDate: string;
}) => api.post<EventRegistration>("/v1/event-registrations", input);

// Records that a member came. checkInTime is "08:42:00". They need not have signed up: they show as a walk-in.
export const checkInMember = (input: {
  memberId: string;
  eventId: string;
  checkInTime: string;
}) => api.post<EventCheckIn>("/v1/event-attendance", input);
