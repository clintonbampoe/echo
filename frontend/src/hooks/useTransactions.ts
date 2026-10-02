import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { transactionsService } from "../services/transactionsService";
import type {
  TransactionFilters,
  TransactionCreatePayload,
  TransactionUpdatePayload,
} from "../types/transaction";

export const useTransactions = (
  filters: TransactionFilters = {},
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["transactions", filters, pageSize, cursor],
    queryFn: () => transactionsService.list(filters, pageSize, cursor),
  });
};

export const useTransaction = (id?: string) => {
  return useQuery({
    queryKey: ["transactions", id],
    queryFn: () => transactionsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: TransactionCreatePayload) =>
      transactionsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["transactions", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });
};

export const useUpdateTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: TransactionUpdatePayload;
    }) => transactionsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["transactions", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });
};

export const useDeleteTransaction = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => transactionsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["transactions", id] });
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
  });
};
