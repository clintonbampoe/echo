import type { RouteObject } from "react-router-dom";

// Pages load on demand, so the visitors code only downloads when someone opens Visitors.
export const visitorRoutes: RouteObject[] = [
  {
    path: "visitors",
    lazy: async () => ({
      Component: (await import("./pages/VisitorsPage")).VisitorsPage,
    }),
  },
  {
    path: "visitors/:id",
    lazy: async () => ({
      Component: (await import("./pages/VisitorPage")).VisitorPage,
    }),
  },
];
