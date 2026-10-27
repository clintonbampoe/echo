import type { TransactionFilters } from "../api/transactions";

export const transactionKeys = {
  all: ["transactions"] as const,
  list: (filters: TransactionFilters) =>
    ["transactions", "list", filters] as const,
  summary: (filters: TransactionFilters) =>
    ["transactions", "summary", filters] as const,
  months: (filters: TransactionFilters) =>
    ["transactions", "months", filters] as const,
  categories: ["transactions", "categories"] as const,
};
