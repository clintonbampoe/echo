import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";

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
    query: { cursor, pageSize: pageSize() },
    signal,
  });

// A member's sign-up for this event. registrationDate is the day they signed up.
export type EventRegistration = {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  registrationDate: string;
};

// Who signed up, in the order they did. The server has no search or filter here, so a page that needs them reads every page.
export const listEventRegistrations = (
  eventId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<EventRegistration>>(`/v1/event-registrations/event/${eventId}`, {
    query: { cursor, pageSize: pageSize() },
    signal,
  });

export const deleteRegistration = (id: string) =>
  api.delete<void>(`/v1/event-registrations/${id}`);

// Signs a member up. registrationDate is the day they signed up. A full event answers 409 with the code EVENT_FULL.
export const registerMember = (input: {
  memberId: string;
  eventId: string;
  registrationDate: string;
}) => api.post<EventRegistration>("/v1/event-registrations", input);
