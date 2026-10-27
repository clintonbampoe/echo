import type { RouteObject } from "react-router-dom";

// Pages load on demand, so the organizations code only downloads when someone opens Organizations.
export const organizationRoutes: RouteObject[] = [
  {
    path: "organizations",
    lazy: async () => ({
      Component: (await import("./pages/OrganizationsPage")).OrganizationsPage,
    }),
  },
];
