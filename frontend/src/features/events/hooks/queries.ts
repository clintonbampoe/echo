import { useQuery } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import { usePagedList } from "@/lib/paged";
import {
  getEventRoster,
  listEventCheckIns,
  listPersonCheckIns,
  type CheckInFilters,
} from "../api/checkIns";
import type { EventQuery } from "../api/events";
import {
  getEvent,
  getEventSummary,
  listEvents,
  listOrganizedEvents,
  listUpcomingEvents,
} from "../api/events";
import {
  listEventRegistrations,
  listPersonRegistrations,
  type RegistrationFilters,
} from "../api/registrations";
import { eventKeys } from "./keys";

export const usePersonRegistrations = (personId: string) =>
  usePagedList(eventKeys.registrations(personId), (c, s) =>
    listPersonRegistrations(personId, c, s),
  );

export const usePersonCheckIns = (personId: string) =>
  usePagedList(eventKeys.checkIns(personId), (c, s) =>
    listPersonCheckIns(personId, c, s),
  );

export const useOrganizedEvents = (organizerId: string) =>
  usePagedList(eventKeys.organized(organizerId), (c, s) =>
    listOrganizedEvents(organizerId, c, s),
  );

// The events an organization is hosting from `today` on.
export const useUpcomingEvents = (organizationId: string, today: string) =>
  usePagedList(eventKeys.upcoming(organizationId, today), (c, s) =>
    listUpcomingEvents(organizationId, today, c, s),
  );

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useEvents = (q: EventQuery) =>
  usePagedList(eventKeys.list(q), (c, s) => listEvents(q, c, s));

export const useEventSummary = (organizationId?: string) =>
  useQuery({
    queryKey: eventKeys.summary(organizationId),
    queryFn: ({ signal }) => getEventSummary(organizationId, signal),
  });

// The event that comes next, for the "Next: Sat 17 Oct" line. Null when nothing is planned.
export const useNextEvent = (
  organizationId: string | undefined,
  today: string,
) =>
  useQuery({
    queryKey: eventKeys.next(organizationId, today),
    queryFn: async ({ signal }) =>
      (
        await listEvents(
          { organizationId, from: today, to: ALL_TIME.to, order: "Asc" },
          undefined,
          signal,
          1,
        )
      ).data[0] ?? null,
  });

export const useEvent = (id: string) =>
  useQuery({
    queryKey: eventKeys.one(id),
    queryFn: ({ signal }) => getEvent(id, signal),
  });

// Who signed up for an event, a page at a time, searched by name and narrowed by whether they came. Each row says
// whether the person came, so the list needs nothing else.
export const useEventRegistrations = (
  eventId: string,
  filters: RegistrationFilters,
) =>
  usePagedList(eventKeys.guests(eventId, filters), (c, s) =>
    listEventRegistrations(eventId, filters, c, s),
  );

// Who came to an event, a page at a time, searched by name and narrowed by whether they had signed up. Each row says
// whether the person was a walk-in.
export const useEventCheckIns = (eventId: string, filters: CheckInFilters) =>
  usePagedList(eventKeys.arrivals(eventId, filters), (c, s) =>
    listEventCheckIns(eventId, filters, c, s),
  );

// How many checked in and how many were walk-ins: the numbers on the event's page, without reading either list.
export const useEventRoster = (eventId: string) =>
  useQuery({
    queryKey: eventKeys.roster(eventId),
    queryFn: ({ signal }) => getEventRoster(eventId, signal),
  });

// Which of these few members are already registered (or already checked in) for the event: for marking the people
// the Record sheet finds, without reading the event's whole list.
export const useAlreadyRecorded = (
  eventId: string,
  mode: "register" | "check-in",
  memberIds: readonly string[],
) =>
  useQuery({
    queryKey: eventKeys.recorded(eventId, mode, memberIds),
    queryFn: async ({ signal }) => {
      const page =
        mode === "register"
          ? await listEventRegistrations(
              eventId,
              { memberIds },
              undefined,
              signal,
            )
          : await listEventCheckIns(eventId, { memberIds }, undefined, signal);
      return new Set(page.data.map((r) => r.memberId));
    },
    enabled: memberIds.length > 0,
  });
