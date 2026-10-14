// The only file in events that knows about TanStack Query.
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const eventKeys = {
  registrations: (id: string) => ["events", "registrations", id] as const,
  checkIns: (id: string) => ["events", "check-ins", id] as const,
  organized: (id: string) => ["events", "organized", id] as const,
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
