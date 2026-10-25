// What other features may use from events. A profile shows what a person did with events and which they
// organize, an organization's page lists what it is hosting; the Events pages (the list and one event) are its routes.
export { eventRoutes } from "./routes";
export { OrganizationEvents } from "./components/profile/OrganizationEvents";
export { OrganizedEvents } from "./components/profile/OrganizedEvents";
export { PersonEvents } from "./components/profile/PersonEvents";
export type { HostedEvent } from "./api/events";
export { usePersonRegistrations, useUpcomingEvents } from "./hooks";
