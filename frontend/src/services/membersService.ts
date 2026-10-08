import { apiFetch } from "./api";
import type {
  Member,
  MemberFilters,
  MemberCreatePayload,
  MemberUpdatePayload,
} from "../types/member";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/members";

export const membersService = {
  list: async (
    filters: MemberFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<Member>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append("Name", filters.name);
    if (filters.status) params.append("Status", filters.status);
    if (filters.gender) params.append("Gender", filters.gender);
    if (filters.joinedDate) params.append("JoinedDate", filters.joinedDate);
    params.append("PageSize", pageSize.toString());
    if (cursor) params.append("Cursor", cursor);

    return apiFetch<PagedResponse<Member>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Member> => {
    return apiFetch<Member>(`${BASE_PATH}/${id}`);
  },

  create: async (data: MemberCreatePayload): Promise<Member> => {
    return apiFetch<Member>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (id: string, data: MemberUpdatePayload): Promise<Member> => {
    return apiFetch<Member>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
