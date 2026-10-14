import { createBrowserRouter, type RouteObject } from "react-router-dom";
import { authRoutes } from "@/features/auth";
import { Dashboard } from "./Dashboard";
import { NotFound } from "./NotFound";
import { RequireAuth } from "./RequireAuth";

// One list, top to bottom, of every URL in the app. Each feature owns its own routes and
// hands them over here; this file only decides where they sit. Pages load on demand.
const routes: RouteObject[] = [
  {
    // Shown for the instant it takes to load the first page's code. Nothing: the page
    // background is already painted, so it just looks like the page arriving.
    HydrateFallback: () => null,
    children: [
      {
        path: "/",
        lazy: async () => ({
          Component: (await import("@/features/landing")).LandingPage,
        }),
      },
      ...authRoutes,
      {
        Component: RequireAuth,
        children: [{ path: "dashboard", Component: Dashboard }],
      },
      { path: "*", Component: NotFound },
    ],
  },
];

export const router = createBrowserRouter(routes);
