import { apiFetch } from "./api";
import type {
  Transaction,
  TransactionFilters,
  TransactionCreatePayload,
  TransactionUpdatePayload,
} from "../types/transaction";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/Transactions";

export const transactionsService = {
  list: async (
    filters: TransactionFilters = {},
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<Transaction>> => {
    const params = new URLSearchParams();
    if (filters.date) params.append("Date", filters.date);
    if (filters.transactionType)
      params.append("TransactionType", filters.transactionType);
    if (filters.categoryId)
      params.append("CategoryId", filters.categoryId.toString());
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<Transaction>>(
      `${BASE_PATH}?${params.toString()}`,
    );
  },

  getById: async (id: string): Promise<Transaction> => {
    return apiFetch<Transaction>(`${BASE_PATH}/${id}`);
  },

  create: async (data: TransactionCreatePayload): Promise<Transaction> => {
    return apiFetch<Transaction>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: TransactionUpdatePayload,
  ): Promise<Transaction> => {
    return apiFetch<Transaction>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
