import { get, getMonthly } from "./client";

// Income minus expenses in the range, and in the stretch of the same length before it. Tithes and project gifts
// are not in it.
export type Net = { net: number; before: number };

export const getNet = (from: string, to: string, signal?: AbortSignal) =>
  get<{
    net: number;
    previousTotalIncome: number;
    previousTotalExpenses: number;
  }>("/v1/transactions/summary", from, to, signal).then((r): Net => ({
    net: r.net,
    before: r.previousTotalIncome - r.previousTotalExpenses,
  }));

// One row per calendar month, oldest first, with zeros for a month with nothing in it.
export type MoneyMonth = {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
};

export const getMonths = (months: number, to: string, signal?: AbortSignal) =>
  getMonthly<MoneyMonth>(
    "/v1/transactions/summary/monthly",
    months,
    to,
    signal,
  );

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
