import { apiFetch } from "./api";
import type {
  ProjectContribution,
  ProjectContributionCreatePayload,
  ProjectContributionUpdatePayload,
  ProjectContributionFilters,
} from "../types/projectContribution";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/ProjectContributions";

export const projectContributionsService = {
  list: async (
    filters: ProjectContributionFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<ProjectContribution>> => {
    const params = new URLSearchParams();
    if (filters.amount) params.append("Amount", filters.amount.toString());
    if (filters.date) params.append("Date", filters.date);
    if (filters.paymentMethod)
      params.append("PaymentMethod", filters.paymentMethod);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch<PagedResponse<ProjectContribution>>(
      `${BASE_PATH}?${params.toString()}`,
    );
  },

  listByProjectId: async (
    projectId: string,
    pageSize: number = 50,
    cursor?: string,
  ): Promise<PagedResponse<ProjectContribution>> => {
    const params = new URLSearchParams();
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);
    return apiFetch<PagedResponse<ProjectContribution>>(
      `${BASE_PATH}/project/${projectId}?${params.toString()}`,
    );
  },

  getById: async (id: string): Promise<ProjectContribution> => {
    return apiFetch<ProjectContribution>(`${BASE_PATH}/${id}`);
  },

  create: async (
    data: ProjectContributionCreatePayload,
  ): Promise<ProjectContribution> => {
    return apiFetch<ProjectContribution>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: ProjectContributionUpdatePayload,
  ): Promise<ProjectContribution> => {
    return apiFetch<ProjectContribution>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
