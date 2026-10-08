import { apiFetch } from "./api";
import type {
  Organization,
  OrganizationSearchResult,
  OrganizationCreatePayload,
  OrganizationUpdatePayload,
} from "../types/organization";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/organizations";

export const organizationsService = {
  list: async (
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<Organization>> => {
    const params = new URLSearchParams();
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<Organization>>(
      `${BASE_PATH}?${params.toString()}`,
    );
  },

  getById: async (id: string): Promise<Organization> => {
    return apiFetch<Organization>(`${BASE_PATH}/${id}`);
  },

  create: async (data: OrganizationCreatePayload): Promise<Organization> => {
    return apiFetch<Organization>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: OrganizationUpdatePayload,
  ): Promise<Organization> => {
    return apiFetch<Organization>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (query: string): Promise<OrganizationSearchResult[]> => {
    return apiFetch<OrganizationSearchResult[]>(
      `${BASE_PATH}/search?q=${encodeURIComponent(query)}`,
    );
  },
};
