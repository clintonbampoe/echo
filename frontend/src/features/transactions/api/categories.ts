import { api } from "@/lib/api";
import type { TransactionType } from "./transactions";

// What a category files transactions under. The type is chosen once, when it is created.
export type Category = {
  id: number;
  name: string;
  categoryType: TransactionType;
  // How many transactions are filed under it. A category in use cannot be deleted, only merged.
  transactionCount: number;
};

export const listCategories = (signal?: AbortSignal) =>
  api.get<Category[]>("/v1/transaction-categories", { signal });

// A new category. The type is chosen now and cannot change later.
export type CategoryInput = { name: string; categoryType: TransactionType };

export const createCategory = (input: CategoryInput) =>
  api.post<Category>("/v1/transaction-categories", input);

// Only the name can be changed. (The API would take a type too, but a category's type is fixed for life.)
export const updateCategory = (id: number, input: { name: string }) =>
  api.put<Category>(`/v1/transaction-categories/${id}`, input);

// A category in use cannot be deleted: the server answers with a conflict.
export const deleteCategory = (id: number) =>
  api.delete<void>(`/v1/transaction-categories/${id}`);

// How many transactions moved, and whether the old category went with them.
export type ReassignResult = { moved: number; sourceDeleted: boolean };

// Moves every transaction of one category to another of the same type, and deletes the first.
export const mergeCategory = (id: number, targetCategoryId: number) =>
  api.post<ReassignResult>(`/v1/transaction-categories/${id}/reassign`, {
    targetCategoryId,
    deleteSource: true,
  });
