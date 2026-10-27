import type { RouteObject } from "react-router-dom";

// Pages load on demand, so the invitations code only downloads when an admin opens it.
export const invitationRoutes: RouteObject[] = [
  {
    path: "invitations",
    lazy: async () => ({
      Component: (await import("./pages/InvitationsPage")).InvitationsPage,
    }),
  },
];
