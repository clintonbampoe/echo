import { useQuery } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import { useEveryRow, usePagedList } from "@/lib/paged";
import { listEventCheckIns, listPersonCheckIns } from "../api/checkIns";
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

// Everyone who signed up for an event, and everyone who came: every page, so the two can be matched.
export const useEventRegistrations = (eventId: string) =>
  useEveryRow(
    usePagedList(eventKeys.guests(eventId), (c, s) =>
      listEventRegistrations(eventId, c, s),
    ),
  );

export const useEventCheckIns = (eventId: string) =>
  useEveryRow(
    usePagedList(eventKeys.arrivals(eventId), (c, s) =>
      listEventCheckIns(eventId, c, s),
    ),
  );
