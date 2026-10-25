import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createCategory,
  deleteCategory,
  mergeCategory,
  updateCategory,
} from "../api/categories";
import type { TransactionUpdate } from "../api/transactions";
import {
  createTransaction,
  deleteTransaction,
  updateTransaction,
} from "../api/transactions";
import { transactionKeys } from "./keys";

// Recording, changing or deleting a transaction changes every list, number and chart on Finance. The category list
// itself did not change, but its counts did: they are marked out of date and fetched when the Categories page opens.
function useRefreshTransactions() {
  const client = useQueryClient();
  // Not awaited: the form that saved moves on at once, and the lists refresh behind it.
  return () => {
    void client.invalidateQueries({
      queryKey: transactionKeys.all,
      predicate: (q) => q.queryKey[1] !== "categories",
    });
    void client.invalidateQueries({
      queryKey: transactionKeys.categories,
      refetchType: "none",
    });
  };
}

export function useCreateTransaction() {
  const refresh = useRefreshTransactions();
  return useMutation({ mutationFn: createTransaction, onSuccess: refresh });
}

export function useUpdateTransaction() {
  const refresh = useRefreshTransactions();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & TransactionUpdate) =>
      updateTransaction(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteTransaction() {
  const refresh = useRefreshTransactions();
  return useMutation({ mutationFn: deleteTransaction, onSuccess: refresh });
}

// A new or deleted category changes only the category list. A rename also changes the name on every row and the
// totals by category, and a merge moves rows, so those refresh everything on Finance.
function useRefreshCategories() {
  const client = useQueryClient();
  return (everything: boolean) =>
    void client.invalidateQueries({
      queryKey: everything ? transactionKeys.all : transactionKeys.categories,
    });
}

export function useCreateCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: createCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      updateCategory(id, { name }),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: deleteCategory,
    onSuccess: () => refresh(false),
  });
}

export function useMergeCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({
      id,
      targetCategoryId,
    }: {
      id: number;
      targetCategoryId: number;
    }) => mergeCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
