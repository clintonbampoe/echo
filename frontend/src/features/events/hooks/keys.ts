import type { CheckInFilters } from "../api/checkIns";
import type { EventQuery } from "../api/events";
import type { RegistrationFilters } from "../api/registrations";

export const eventKeys = {
  all: ["events"] as const,
  list: (q: EventQuery) => ["events", "list", q] as const,
  summary: (organizationId?: string) =>
    ["events", "summary", organizationId] as const,
  next: (organizationId: string | undefined, today: string) =>
    ["events", "next", organizationId, today] as const,
  one: (id: string) => ["events", "one", id] as const,
  guests: (id: string, filters: RegistrationFilters) =>
    ["events", "guests", id, filters] as const,
  arrivals: (id: string, filters: CheckInFilters) =>
    ["events", "arrivals", id, filters] as const,
  roster: (id: string) => ["events", "roster", id] as const,
  recorded: (id: string, mode: string, memberIds: readonly string[]) =>
    ["events", "recorded", id, mode, memberIds] as const,
  registrations: (id: string) => ["events", "registrations", id] as const,
  checkIns: (id: string) => ["events", "check-ins", id] as const,
  organized: (id: string) => ["events", "organized", id] as const,
  upcoming: (organizationId: string, from: string) =>
    ["events", "upcoming", organizationId, from] as const,
};
