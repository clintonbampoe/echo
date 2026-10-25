import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

export type PaymentMethod =
  "Cash" | "Cheque" | "CreditCard" | "MobileMoney" | "BankTransfer";

// Money in, or money out. A transaction's type is always its category's type.
export type TransactionType = "Income" | "Expense";

// One payment the church made or received.
export type Transaction = {
  id: string;
  categoryId: number;
  categoryName: string;
  transactionType: TransactionType;
  transactionDate: string;
  amount: number;
  description?: string | null;
  createdAt: string;
};

// What the Finance page narrows the list by. `from` and `to` are the days a transaction is dated.
// With no dates the server answers for this week, so the page always sends them.
export type TransactionFilters = {
  transactionType?: TransactionType;
  categoryId?: number;
  from: string;
  to: string;
};

export const listTransactions = (
  filters: TransactionFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Transaction>>("/v1/transactions", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// What one category added up to over the period, and how many transactions it holds.
export type CategoryTotal = {
  categoryId: number;
  categoryName: string;
  type: TransactionType;
  total: number;
  count: number;
};

// The numbers above the list. They follow the same filters.
export type TransactionSummary = {
  totalIncome: number;
  totalExpenses: number;
  net: number;
  mostActiveCategory?: string | null;
  byCategory: CategoryTotal[];
  window?: DateWindow | null;
};

export const getTransactionSummary = (
  filters: TransactionFilters,
  signal?: AbortSignal,
) =>
  api.get<TransactionSummary>("/v1/transactions/summary", {
    query: { ...filters },
    signal,
  });

// One row per calendar month in the range, so the chart comes from a single request.
export type TransactionMonth = {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
  net: number;
};

export const getTransactionMonths = (
  filters: TransactionFilters,
  signal?: AbortSignal,
) =>
  api.get<TransactionMonth[]>("/v1/transactions/summary/monthly", {
    query: { ...filters },
    signal,
  });

// What the API accepts when recording a transaction. The type must be the type of the category it is filed under.
export type TransactionInput = {
  categoryId: number;
  transactionType: TransactionType;
  transactionDate: string;
  amount: number;
  description?: string;
};

// An update sends only what changed, but the form always sends all of it. An empty description clears the note.
export type TransactionUpdate = Partial<TransactionInput>;

export const createTransaction = (input: TransactionInput) =>
  api.post<Transaction>("/v1/transactions", input);

export const updateTransaction = (id: string, input: TransactionUpdate) =>
  api.put<Transaction>(`/v1/transactions/${id}`, input);

export const deleteTransaction = (id: string) =>
  api.delete<void>(`/v1/transactions/${id}`);
