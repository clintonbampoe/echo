import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

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

// A member's check-in at this event. checkInTime is a time of day ("08:42:00"); the day is the event's.
export type EventCheckIn = {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  checkInTime: string;
};

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
