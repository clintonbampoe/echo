import { apiFetch } from "./api";
import type {
  Tithe,
  PagedResponse,
  TitheFilters,
  TitheCreatePayload,
  TitheUpdatePayload,
} from "../types/tithe";

const BASE_PATH = "/v1/tithes";

export const tithesService = {
  list: async (
    filters: TitheFilters = {},
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<Tithe>> => {
    const params = new URLSearchParams();
    if (filters.year) params.append("Year", filters.year.toString());
    if (filters.month) params.append("Month", filters.month);
    if (filters.paymentMethod)
      params.append("PaymentMethod", filters.paymentMethod);
    if (filters.memberId) params.append("MemberId", filters.memberId);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<Tithe>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Tithe> => {
    return apiFetch<Tithe>(`${BASE_PATH}/${id}`);
  },

  create: async (data: TitheCreatePayload): Promise<Tithe> => {
    return apiFetch<Tithe>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: TitheUpdatePayload): Promise<Tithe> => {
    return apiFetch<Tithe>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: "DELETE",
    });
  },
};
