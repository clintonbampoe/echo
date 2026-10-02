import type { PagedResponse } from "./pagination";

export type TransactionType = "Income" | "Expense";

export interface TransactionCreatePayload {
  categoryId: number;
  transactionType: TransactionType;
  transactionDate: string; // YYYY-MM-DD
  amount: number;
  description?: string | null;
}

export type TransactionUpdatePayload = Partial<TransactionCreatePayload>;

export interface Transaction {
  id: string;
  categoryId: number;
  categoryName: string;
  transactionType: TransactionType;
  transactionDate: string;
  amount: number;
  description?: string | null;
  createdAt: string;
}

export interface TransactionFilters {
  date?: string; // YYYY-MM-DD
  transactionType?: TransactionType;
  categoryId?: number;
}

export interface CategoryStream {
  category: {
    id: number;
    name: string;
    categoryType: TransactionType;
  };
  totalAmount: number;
  percentOfTotal: number;
  transactionCount: number;
}

// UI form types — move to component folder when components are refactored.
export interface RecordTransactionForm {
  transactionType: TransactionType;
  amount: string;
  categoryId: number | "";
  transactionDate: string;
  description: string;
}

export type { PagedResponse };
