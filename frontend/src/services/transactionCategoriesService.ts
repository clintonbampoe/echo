import { apiFetch } from "./api";
import type {
  TransactionCategory,
  TransactionCategoryCreatePayload,
  TransactionCategoryUpdatePayload,
  TransactionCategorySearchResult,
} from "../types/transactionCategory";

const BASE_PATH = "/v1/transaction-categories";

export const transactionCategoriesService = {
  list: async (): Promise<TransactionCategory[]> => {
    return apiFetch<TransactionCategory[]>(BASE_PATH);
  },

  getById: async (id: number): Promise<TransactionCategory> => {
    return apiFetch<TransactionCategory>(`${BASE_PATH}/${id}`);
  },

  create: async (
    data: TransactionCategoryCreatePayload,
  ): Promise<TransactionCategory> => {
    return apiFetch<TransactionCategory>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: number,
    data: TransactionCategoryUpdatePayload,
  ): Promise<TransactionCategory> => {
    return apiFetch<TransactionCategory>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: number): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (q: string): Promise<TransactionCategorySearchResult[]> => {
    return apiFetch<TransactionCategorySearchResult[]>(
      `${BASE_PATH}/search?q=${encodeURIComponent(q)}`,
    );
  },
};
