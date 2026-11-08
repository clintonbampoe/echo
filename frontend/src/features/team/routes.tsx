import type { RouteObject } from "react-router-dom";

// The page loads on demand, so the team code only downloads when an admin opens it.
export const teamRoutes: RouteObject[] = [
  {
    path: "team",
    lazy: async () => ({
      Component: (await import("./pages/TeamPage")).TeamPage,
    }),
  },
];
