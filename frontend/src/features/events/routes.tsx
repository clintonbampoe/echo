import type { RouteObject } from "react-router-dom";

const page = (tab: "upcoming" | "past") => async () => {
  const { EventsPage } = await import("./pages/EventsPage");
  return { Component: () => <EventsPage tab={tab} /> };
};

// Each tab is its own address. The pages load on demand, so the events code only downloads when someone opens Events.
export const eventRoutes: RouteObject[] = [
  { path: "events", lazy: page("upcoming") },
  { path: "events/past", lazy: page("past") },
  {
    path: "events/:id",
    lazy: async () => ({
      Component: (await import("./pages/EventPage")).EventPage,
    }),
  },
];
