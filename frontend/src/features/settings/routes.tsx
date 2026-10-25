import type { RouteObject } from "react-router-dom";

export const settingsRoutes: RouteObject[] = [
  {
    path: "settings",
    lazy: async () => ({
      Component: (await import("./pages/SettingsPage")).SettingsPage,
    }),
  },
];
