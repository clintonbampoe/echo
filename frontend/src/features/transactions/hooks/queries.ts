import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import { listCategories } from "../api/categories";
import type { TransactionFilters } from "../api/transactions";
import {
  getTransactionMonths,
  getTransactionSummary,
  listTransactions,
} from "../api/transactions";
import { transactionKeys } from "./keys";

const FIVE_MINUTES = 5 * 60_000;

// Every transaction that matches the filters, newest first.
export const useTransactions = (filters: TransactionFilters) =>
  usePagedList(transactionKeys.list(filters), (c, s) =>
    listTransactions(filters, c, s),
  );

export const useTransactionSummary = (filters: TransactionFilters) =>
  useQuery({
    queryKey: transactionKeys.summary(filters),
    queryFn: ({ signal }) => getTransactionSummary(filters, signal),
  });

export const useTransactionMonths = (filters: TransactionFilters) =>
  useQuery({
    queryKey: transactionKeys.months(filters),
    queryFn: ({ signal }) => getTransactionMonths(filters, signal),
  });

// Every category, for the filter and the form. They change rarely, so they are kept for a few minutes.
export const useCategories = () =>
  useQuery({
    queryKey: transactionKeys.categories,
    queryFn: ({ signal }) => listCategories(signal),
    staleTime: FIVE_MINUTES,
  });
