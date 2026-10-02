import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { transactionCategoriesService } from "../services/transactionCategoriesService";
import type {
  TransactionCategoryCreatePayload,
  TransactionCategoryUpdatePayload,
} from "../types/transactionCategory";

export const useTransactionCategories = () => {
  return useQuery({
    queryKey: ["transactionCategories"],
    queryFn: () => transactionCategoriesService.list(),
  });
};

export const useCreateTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: TransactionCategoryCreatePayload) =>
      transactionCategoriesService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["transactionCategories", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
    },
  });
};

export const useUpdateTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: TransactionCategoryUpdatePayload;
    }) => transactionCategoriesService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["transactionCategories", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
    },
  });
};

export const useDeleteTransactionCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => transactionCategoriesService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["transactionCategories", id] });
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
    },
  });
};
