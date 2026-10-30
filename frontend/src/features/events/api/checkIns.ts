import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";

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
    query: { cursor, pageSize: pageSize() },
    signal,
  });

// A member's check-in at this event. checkInTime is a time of day ("08:42:00"); the day is the event's. In this event's
// list, `walkIn` says they came without signing up and `registrationId` is their sign-up when they had one; on any
// other route neither is filled in.
export type EventCheckIn = {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  checkInTime: string;
  walkIn?: boolean | null;
  registrationId?: string | null;
};

// What narrows an event's check-ins: part of a name, whether they had signed up, or only these members (to ask which
// of a few people have already come).
export type CheckInFilters = {
  name?: string;
  kind?: "Registered" | "WalkIn";
  memberIds?: readonly string[];
};

// Who came, in the order they arrived, a page at a time. The server searches by name and narrows by whether they had
// signed up, so a page reads only the rows it shows.
export const listEventCheckIns = (
  eventId: string,
  filters: CheckInFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<EventCheckIn>>(`/v1/event-attendance/event/${eventId}`, {
    query: { ...filters, cursor, pageSize: pageSize() },
    signal,
  });

// How many came, and how many of them had not signed up. The sign-ups are `registrationCount` on the event.
export type EventRoster = {
  checkedIn: number;
  walkIns: number;
  cameRegistered: number;
};

export const getEventRoster = (eventId: string, signal?: AbortSignal) =>
  api.get<EventRoster>(`/v1/event-attendance/event/${eventId}/summary`, {
    signal,
  });

export const deleteCheckIn = (id: string) =>
  api.delete<void>(`/v1/event-attendance/${id}`);

// The time can be changed, not taken away.
export const updateCheckInTime = (id: string, checkInTime: string) =>
  api.put<EventCheckIn>(`/v1/event-attendance/${id}`, { checkInTime });

// Records that a member came. checkInTime is "08:42:00". They need not have signed up: they show as a walk-in.
export const checkInMember = (input: {
  memberId: string;
  eventId: string;
  checkInTime: string;
}) => api.post<EventCheckIn>("/v1/event-attendance", input);
