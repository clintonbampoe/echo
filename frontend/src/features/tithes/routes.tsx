import type { RouteObject } from "react-router-dom";

// The page loads on demand, so the tithes code only downloads when someone opens Tithes.
export const titheRoutes: RouteObject[] = [
  {
    path: "tithes",
    lazy: async () => ({
      Component: (await import("./components/TithesPage")).TithesPage,
    }),
  },
];
