import { createBrowserRouter, type RouteObject } from "react-router-dom";
import { accountRoutes } from "@/features/account";
import { assetRoutes } from "@/features/assets";
import { attendanceRoutes } from "@/features/attendance";
import { authRoutes } from "@/features/auth";
import { dashboardRoutes } from "@/features/dashboard";
import { eventRoutes } from "@/features/events";
import { invitationRoutes } from "@/features/invitations";
import { memberRoutes } from "@/features/members";
import { organizationRoutes } from "@/features/organizations";
import { settingsRoutes } from "@/features/settings";
import { titheRoutes } from "@/features/tithes";
import { projectRoutes } from "@/features/projects";
import { transactionRoutes } from "@/features/transactions";
import { visitorRoutes } from "@/features/visitors";
import { websiteRoutes } from "@/features/website";
import { pageRoutes } from "@/pages";
import { LoadingScreen } from "@/ui";
import { PageRouteError, ShellRouteError } from "./errors";
import { NotFound } from "./NotFound";
import { RequireAuth } from "./RequireAuth";
import { AppShell } from "./shell";

// One list, top to bottom, of every URL in the app. Each feature owns its own routes and
// hands them over here; this file only decides where they sit. Pages load on demand.
const routes: RouteObject[] = [
  {
    // Shown while the first page's code loads. It only appears after a short wait, so a quick
    // load never flashes it.
    HydrateFallback: () => <LoadingScreen />,
    // A page that fails outside the app (the website, sign in) is a page of its own.
    ErrorBoundary: PageRouteError,
    children: [
      ...websiteRoutes,
      ...authRoutes,
      {
        Component: RequireAuth,
        children: [
          {
            Component: AppShell,
            children: [
              {
                // A page that fails inside the app keeps the sidebar, so there is a way out.
                ErrorBoundary: ShellRouteError,
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
                  ...invitationRoutes,
                  ...settingsRoutes,
                  ...accountRoutes,
                  ...pageRoutes,
                ],
              },
            ],
          },
        ],
      },
      { path: "*", Component: NotFound },
    ],
  },
];

export const router = createBrowserRouter(routes);
