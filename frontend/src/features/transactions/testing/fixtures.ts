import type { Category } from "../api/categories";
import type { Transaction } from "../api/transactions";

export const aTransaction = (over: Partial<Transaction> = {}): Transaction => ({
  id: "t1",
  categoryId: 2,
  categoryName: "Utilities",
  transactionType: "Expense",
  transactionDate: "2026-10-13",
  amount: 1240,
  description: null,
  createdAt: "2026-10-13T09:00:00Z",
  ...over,
});

export const aCategory = (over: Partial<Category> = {}): Category => ({
  id: 2,
  name: "Utilities",
  categoryType: "Expense",
  transactionCount: 14,
  ...over,
});
