import { apiFetch } from "./api";
import type {
  Project,
  ProjectCreatePayload,
  ProjectUpdatePayload,
  ProjectFilters,
  ProjectSearchResult,
} from "../types/project";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/projects";

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
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },

  search: async (name: string): Promise<ProjectSearchResult[]> => {
    return apiFetch<ProjectSearchResult[]>(
      `${BASE_PATH}/search?name=${encodeURIComponent(name)}`,
    );
  },
};
