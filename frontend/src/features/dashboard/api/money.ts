import { get } from "./client";

// Income minus expenses. Tithes and project gifts are not in it.
export const getNet = (from: string, to: string, signal?: AbortSignal) =>
  get<{ net: number }>("/v1/transactions/summary", from, to, signal).then(
    (r) => r.net,
  );

// One row per calendar month in the range, oldest first, with zeros for a month with nothing in it.
export type MoneyMonth = {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
};

export const getMonths = (from: string, to: string, signal?: AbortSignal) =>
  get<MoneyMonth[]>("/v1/transactions/summary/monthly", from, to, signal);

// What one category added up to over the range.
export type CategoryTotal = {
  categoryId: number;
  categoryName: string;
  type: "Income" | "Expense";
  total: number;
};

export const getCategories = (from: string, to: string, signal?: AbortSignal) =>
  get<{ byCategory: CategoryTotal[] }>(
    "/v1/transactions/summary",
    from,
    to,
    signal,
  ).then((r) => r.byCategory);
