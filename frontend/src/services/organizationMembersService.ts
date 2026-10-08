import { apiFetch } from "./api";
import type {
  OrganizationMember,
  OrganizationMemberFilters,
  OrganizationMemberCreatePayload,
  OrganizationMemberUpdatePayload,
} from "../types/organizationMember";
import type { PagedResponse } from "../types/pagination";

const BASE_PATH = "/v1/OrganizationMember";

const buildParams = (
  filters: OrganizationMemberFilters,
  pageSize: number,
  cursor?: string,
): string => {
  const params = new URLSearchParams();
  if (filters.role) params.append("Role", filters.role);
  params.append("PageSize", pageSize.toString());
  if (cursor) params.append("Cursor", cursor);
  return params.toString();
};

export const organizationMembersService = {
  list: async (
    filters: OrganizationMemberFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<OrganizationMember>> => {
    return apiFetch<PagedResponse<OrganizationMember>>(
      `${BASE_PATH}?${buildParams(filters, pageSize, cursor)}`,
    );
  },

  listByOrganization: async (
    organizationId: string,
    filters: OrganizationMemberFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<OrganizationMember>> => {
    return apiFetch<PagedResponse<OrganizationMember>>(
      `${BASE_PATH}/organizations/${organizationId}?${buildParams(filters, pageSize, cursor)}`,
    );
  },

  listByMember: async (
    memberId: string,
    filters: OrganizationMemberFilters = {},
    pageSize: number = 24,
    cursor?: string,
  ): Promise<PagedResponse<OrganizationMember>> => {
    return apiFetch<PagedResponse<OrganizationMember>>(
      `${BASE_PATH}/member/${memberId}?${buildParams(filters, pageSize, cursor)}`,
    );
  },

  create: async (
    data: OrganizationMemberCreatePayload,
  ): Promise<OrganizationMember> => {
    return apiFetch<OrganizationMember>(BASE_PATH, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  update: async (
    id: string,
    data: OrganizationMemberUpdatePayload,
  ): Promise<OrganizationMember> => {
    return apiFetch<OrganizationMember>(`${BASE_PATH}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, { method: "DELETE" });
  },
};
