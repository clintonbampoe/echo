import type { EventQuery } from "../api/events";

export const eventKeys = {
  all: ["events"] as const,
  list: (q: EventQuery) => ["events", "list", q] as const,
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
