// The only file in transactions that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

const FIVE_MINUTES = 5 * 60_000;

export const transactionKeys = {
  all: ["transactions"] as const,
  list: (filters: api.TransactionFilters) =>
    ["transactions", "list", filters] as const,
  summary: (filters: api.TransactionFilters) =>
    ["transactions", "summary", filters] as const,
  months: (filters: api.TransactionFilters) =>
    ["transactions", "months", filters] as const,
  categories: ["transactions", "categories"] as const,
};

// Every transaction that matches the filters, newest first.
export const useTransactions = (filters: api.TransactionFilters) =>
  usePagedList(transactionKeys.list(filters), (c, s) =>
    api.listTransactions(filters, c, s),
  );

export const useTransactionSummary = (filters: api.TransactionFilters) =>
  useQuery({
    queryKey: transactionKeys.summary(filters),
    queryFn: ({ signal }) => api.getTransactionSummary(filters, signal),
  });

export const useTransactionMonths = (filters: api.TransactionFilters) =>
  useQuery({
    queryKey: transactionKeys.months(filters),
    queryFn: ({ signal }) => api.getTransactionMonths(filters, signal),
  });

// Every category, for the filter and the form. They change rarely, so they are kept for a few minutes.
export const useCategories = () =>
  useQuery({
    queryKey: transactionKeys.categories,
    queryFn: ({ signal }) => api.listCategories(signal),
    staleTime: FIVE_MINUTES,
  });

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
  return useMutation({ mutationFn: api.createTransaction, onSuccess: refresh });
}

export function useUpdateTransaction() {
  const refresh = useRefreshTransactions();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & api.TransactionUpdate) =>
      api.updateTransaction(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteTransaction() {
  const refresh = useRefreshTransactions();
  return useMutation({ mutationFn: api.deleteTransaction, onSuccess: refresh });
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
    mutationFn: api.createCategory,
    onSuccess: () => refresh(false),
  });
}

export function useRenameCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      api.updateCategory(id, { name }),
    onSuccess: () => refresh(true),
  });
}

export function useDeleteCategory() {
  const refresh = useRefreshCategories();
  return useMutation({
    mutationFn: api.deleteCategory,
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
    }) => api.mergeCategory(id, targetCategoryId),
    onSuccess: () => refresh(true),
  });
}
