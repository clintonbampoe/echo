import type { RouteObject } from "react-router-dom";

// Pages load on demand, so the members code only downloads when someone opens Members.
export const memberRoutes: RouteObject[] = [
  {
    path: "members",
    lazy: async () => ({
      Component: (await import("./pages/MembersPage")).MembersPage,
    }),
  },
];
