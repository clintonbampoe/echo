// The only file in events that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import { useEveryRow, usePagedList } from "@/lib/paged";
import * as api from "./api";

export const eventKeys = {
  all: ["events"] as const,
  list: (q: api.EventQuery) => ["events", "list", q] as const,
  summary: (organizationId?: string) =>
    ["events", "summary", organizationId] as const,
  next: (organizationId: string | undefined, today: string) =>
    ["events", "next", organizationId, today] as const,
  one: (id: string) => ["events", "one", id] as const,
  guests: (id: string) => ["events", "guests", id] as const,
  arrivals: (id: string) => ["events", "arrivals", id] as const,
  registrations: (id: string) => ["events", "registrations", id] as const,
  checkIns: (id: string) => ["events", "check-ins", id] as const,
  organized: (id: string) => ["events", "organized", id] as const,
  upcoming: (organizationId: string, from: string) =>
    ["events", "upcoming", organizationId, from] as const,
};

export const usePersonRegistrations = (personId: string) =>
  usePagedList(eventKeys.registrations(personId), (c, s) =>
    api.listPersonRegistrations(personId, c, s),
  );

export const usePersonCheckIns = (personId: string) =>
  usePagedList(eventKeys.checkIns(personId), (c, s) =>
    api.listPersonCheckIns(personId, c, s),
  );

export const useOrganizedEvents = (organizerId: string) =>
  usePagedList(eventKeys.organized(organizerId), (c, s) =>
    api.listOrganizedEvents(organizerId, c, s),
  );

// The events an organization is hosting from `today` on.
export const useUpcomingEvents = (organizationId: string, today: string) =>
  usePagedList(eventKeys.upcoming(organizationId, today), (c, s) =>
    api.listUpcomingEvents(organizationId, today, c, s),
  );

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useEvents = (q: api.EventQuery) =>
  usePagedList(eventKeys.list(q), (c, s) => api.listEvents(q, c, s));

export const useEventSummary = (organizationId?: string) =>
  useQuery({
    queryKey: eventKeys.summary(organizationId),
    queryFn: ({ signal }) => api.getEventSummary(organizationId, signal),
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
        await api.listEvents(
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
    queryFn: ({ signal }) => api.getEvent(id, signal),
  });

// Everyone who signed up for an event, and everyone who came: every page, so the two can be matched.
export const useEventRegistrations = (eventId: string) =>
  useEveryRow(
    usePagedList(eventKeys.guests(eventId), (c, s) =>
      api.listEventRegistrations(eventId, c, s),
    ),
  );

export const useEventCheckIns = (eventId: string) =>
  useEveryRow(
    usePagedList(eventKeys.arrivals(eventId), (c, s) =>
      api.listEventCheckIns(eventId, c, s),
    ),
  );

// Adding, changing or removing an event changes the lists, the counts, the summary, and every page that shows an
// event of someone's. One refresh covers them all.
function useRefreshEvents() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: eventKeys.all });
}

export function useCreateEvent() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: api.createEvent, onSuccess: refresh });
}

export function useUpdateEvent(id: string) {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: (input: Partial<api.EventInput>) => api.updateEvent(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteEvent() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: api.deleteEvent, onSuccess: refresh });
}

export function useRemoveRegistration() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: api.deleteRegistration,
    onSuccess: refresh,
  });
}

export function useRemoveCheckIn() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: api.deleteCheckIn, onSuccess: refresh });
}

export function useUpdateCheckInTime() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: ({ id, time }: { id: string; time: string }) =>
      api.updateCheckInTime(id, time),
    onSuccess: refresh,
  });
}

// Recording a person at the door is quick and repeated, so the lists refresh behind it instead of holding it up. Settled,
// not just success: a refusal (the event filled up, or they were already recorded) means someone else changed it.
export function useRegisterMember() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: api.registerMember,
    onSettled: () => void refresh(),
  });
}

export function useCheckInMember() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: api.checkInMember,
    onSettled: () => void refresh(),
  });
}
