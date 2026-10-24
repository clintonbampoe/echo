import { createBrowserRouter, type RouteObject } from "react-router-dom";
import { assetRoutes } from "@/features/assets";
import { attendanceRoutes } from "@/features/attendance";
import { authRoutes } from "@/features/auth";
import { dashboardRoutes } from "@/features/dashboard";
import { eventRoutes } from "@/features/events";
import { memberRoutes } from "@/features/members";
import { organizationRoutes } from "@/features/organizations";
import { titheRoutes } from "@/features/tithes";
import { projectRoutes } from "@/features/projects";
import { transactionRoutes } from "@/features/transactions";
import { visitorRoutes } from "@/features/visitors";
import { websiteRoutes } from "@/features/website";
import { NotFound } from "./NotFound";
import { RequireAuth } from "./RequireAuth";
import { AppShell } from "./shell";

// One list, top to bottom, of every URL in the app. Each feature owns its own routes and
// hands them over here; this file only decides where they sit. Pages load on demand.
const routes: RouteObject[] = [
  {
    // Shown for the instant it takes to load the first page's code. Nothing: the page
    // background is already painted, so it just looks like the page arriving.
    HydrateFallback: () => null,
    children: [
      ...websiteRoutes,
      ...authRoutes,
      {
        Component: RequireAuth,
        children: [
          {
            Component: AppShell,
            children: [
              ...dashboardRoutes,
              ...memberRoutes,
              ...visitorRoutes,
              ...organizationRoutes,
              ...titheRoutes,
              ...attendanceRoutes,
              ...eventRoutes,
              ...transactionRoutes,
              ...projectRoutes,
              ...assetRoutes,
            ],
          },
        ],
      },
      { path: "*", Component: NotFound },
    ],
  },
];

export const router = createBrowserRouter(routes);
