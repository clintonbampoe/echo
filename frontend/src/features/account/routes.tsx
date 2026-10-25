import type { RouteObject } from "react-router-dom";

export const accountRoutes: RouteObject[] = [
  {
    path: "me",
    lazy: async () => ({
      Component: (await import("./pages/MePage")).MePage,
    }),
  },
];
