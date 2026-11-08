import type { RouteObject } from "react-router-dom";

// The pages load on demand, so the staff code only downloads when an admin opens it.
export const staffRoutes: RouteObject[] = [
  {
    path: "staff",
    lazy: async () => ({
      Component: (await import("./pages/StaffPage")).StaffPage,
    }),
  },
  {
    path: "staff/:role",
    lazy: async () => ({
      Component: (await import("./pages/StaffRolePage")).StaffRolePage,
    }),
  },
];
