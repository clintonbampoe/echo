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
