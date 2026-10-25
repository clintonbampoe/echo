import type { RouteObject } from "react-router-dom";

// The page loads on demand, so the dashboard code only downloads when someone opens it.
export const dashboardRoutes: RouteObject[] = [
  {
    path: "dashboard",
    lazy: async () => ({
      Component: (await import("./pages/DashboardPage")).DashboardPage,
    }),
  },
];
