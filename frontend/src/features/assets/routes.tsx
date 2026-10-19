import type { RouteObject } from "react-router-dom";

// The Assets pages load on demand, so their code only downloads when someone opens Assets.
export const assetRoutes: RouteObject[] = [
  {
    path: "assets",
    lazy: async () => ({
      Component: (await import("./components/AssetsPage")).AssetsPage,
    }),
  },
  {
    path: "assets/categories",
    lazy: async () => ({
      Component: (await import("./components/AssetCategoriesPage"))
        .AssetCategoriesPage,
    }),
  },
  {
    path: "assets/:id",
    lazy: async () => ({
      Component: (await import("./components/AssetPage")).AssetPage,
    }),
  },
];
