import type { RouteObject } from "react-router-dom";

// The Finance pages load on demand, so their code only downloads when someone opens Finance.
export const transactionRoutes: RouteObject[] = [
  {
    path: "finance",
    lazy: async () => ({
      Component: (await import("./pages/FinancePage")).FinancePage,
    }),
  },
  {
    path: "finance/categories",
    lazy: async () => ({
      Component: (await import("./pages/CategoriesPage")).CategoriesPage,
    }),
  },
];
