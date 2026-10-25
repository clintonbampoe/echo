import type { RouteObject } from "react-router-dom";

// The screens that stitch several features together: one person's profile, one visitor, one organization.
// Pages load on demand, like each feature's own.
export const pageRoutes: RouteObject[] = [
  {
    path: "members/:id",
    lazy: async () => ({
      Component: (await import("./member/MemberPage")).MemberPage,
    }),
  },
  {
    path: "visitors/:id",
    lazy: async () => ({
      Component: (await import("./visitor/VisitorPage")).VisitorPage,
    }),
  },
  {
    path: "organizations/:id",
    lazy: async () => ({
      Component: (await import("./organization/OrganizationPage"))
        .OrganizationPage,
    }),
  },
];
