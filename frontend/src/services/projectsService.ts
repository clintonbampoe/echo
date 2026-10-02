import { apiFetch } from "./api";
import type {
  Project,
  PagedResponse,
  ProjectFilters,
  ProjectCreatePayload,
  ProjectUpdatePayload,
} from "../types/project";

const BASE_PATH = "/v1/Projects";

export const projectsService = {
  list: async (
    filters: ProjectFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<Project>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append("Name", filters.name);
    if (filters.status) params.append("Status", filters.status);
    if (filters.categoryId)
      params.append("CategoryId", filters.categoryId.toString());
    if (filters.startDate) params.append("StartDate", filters.startDate);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<Project>>(
      `${BASE_PATH}?${params.toString()}`,
    );
  },

  getById: async (id: string): Promise<Project> => {
    return apiFetch<Project>(`${BASE_PATH}/${id}`);
  },

  create: async (data: ProjectCreatePayload): Promise<Project> => {
    return apiFetch<Project>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: ProjectUpdatePayload): Promise<Project> => {
    return apiFetch<Project>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: "DELETE",
    });
  },

  search: async (
    query: string,
  ): Promise<Array<{ id: string; name: string }>> => {
    return apiFetch<Array<{ id: string; name: string }>>(
      `${BASE_PATH}/search?q=${encodeURIComponent(query)}`,
    );
  },
};
