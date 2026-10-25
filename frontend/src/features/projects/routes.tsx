import type { RouteObject } from "react-router-dom";

// The Projects pages load on demand, so their code only downloads when someone opens Projects.
export const projectRoutes: RouteObject[] = [
  {
    path: "projects",
    lazy: async () => ({
      Component: (await import("./pages/ProjectsPage")).ProjectsPage,
    }),
  },
  {
    path: "projects/categories",
    lazy: async () => ({
      Component: (await import("./pages/ProjectCategoriesPage"))
        .ProjectCategoriesPage,
    }),
  },
  {
    path: "projects/:id",
    lazy: async () => ({
      Component: (await import("./pages/ProjectPage")).ProjectPage,
    }),
  },
];
