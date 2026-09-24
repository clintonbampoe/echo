import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { transactionsService } from '../services/transactionsService';
import { transactionCategoriesService } from '../services/transactionCategoriesService';
import type { Transaction, TransactionFilters, TransactionType } from '../types/finance';

export const useTransactions = (filters: TransactionFilters = {}, pageSize: number = 50, cursor?: string) => {
  return useQuery({
    queryKey: ['transactions', filters, pageSize, cursor],
    queryFn: () => transactionsService.list(filters, pageSize, cursor),
  });
};

export const useTransaction = (id?: string) => {
  return useQuery({
    queryKey: ['transactions', id],
    queryFn: () => (id ? transactionsService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Transaction>) => transactionsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });
};

export const useUpdateTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Transaction> }) =>
      transactionsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['transactions', variables.id] });
    },
  });
};

export const useDeleteTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => transactionsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });
};

// Transaction Categories
export const useTransactionCategories = () => {
  return useQuery({
    queryKey: ['transactionCategories'],
    queryFn: () => transactionCategoriesService.list(),
  });
};

export const useCreateTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string; categoryType: TransactionType }) =>
      transactionCategoriesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactionCategories'] });
    },
  });
};

export const useUpdateTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: { name?: string; categoryType?: TransactionType } }) =>
      transactionCategoriesService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactionCategories'] });
    },
  });
};

export const useDeleteTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => transactionCategoriesService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactionCategories'] });
    },
  });
};
