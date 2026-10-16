import { createBrowserRouter, type RouteObject } from "react-router-dom";
import { attendanceRoutes } from "@/features/attendance";
import { authRoutes } from "@/features/auth";
import { memberRoutes } from "@/features/members";
import { organizationRoutes } from "@/features/organizations";
import { titheRoutes } from "@/features/tithes";
import { visitorRoutes } from "@/features/visitors";
import { ComingSoon } from "./ComingSoon";
import { Dashboard } from "./Dashboard";
import { NotFound } from "./NotFound";
import { RequireAuth } from "./RequireAuth";
import { AppShell } from "./shell";

// Pages that lists already link to, before their features exist. Each one is replaced by the real route.
const placeholders: RouteObject[] = [
  {
    path: "events/:id",
    element: <ComingSoon icon="calendar" title="Event page" />,
  },
  {
    path: "projects/:id",
    element: <ComingSoon icon="projects" title="Project page" />,
  },
];

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
        children: [
          {
            Component: AppShell,
            children: [
              { path: "dashboard", Component: Dashboard },
              ...memberRoutes,
              ...visitorRoutes,
              ...organizationRoutes,
              ...titheRoutes,
              ...attendanceRoutes,
              ...placeholders,
            ],
          },
        ],
      },
      { path: "*", Component: NotFound },
    ],
  },
];

export const router = createBrowserRouter(routes);
