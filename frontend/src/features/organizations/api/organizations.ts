import { api, type DateWindow, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";

export type Organization = {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string;
  memberCount: number;
};

// The list is by the day an organization was created. No from/to means this year.
export type OrganizationFilters = {
  name?: string;
  from?: string;
  to?: string;
};

// The numbers above the list.
export type OrganizationSummary = {
  totalOrganizations: number;
  // Memberships across all organizations: one person in two groups counts twice.
  totalMembers: number;
  averageMembersPerOrganization: number;
  // The name of the group with the most members. The server sends only the name.
  largestOrganization?: string | null;
  window?: DateWindow | null;
};

export const listOrganizations = (
  filters: OrganizationFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Organization>>("/v1/organizations", {
    query: { ...filters, cursor, pageSize: pageSize() },
    signal,
  });

export const getOrganization = (id: string, signal?: AbortSignal) =>
  api.get<Organization>(`/v1/organizations/${id}`, { signal });

// The numbers describe the time range only, so the search does not change them.
export const getOrganizationSummary = (
  filters: Pick<OrganizationFilters, "from" | "to">,
  signal?: AbortSignal,
) =>
  api.get<OrganizationSummary>("/v1/organizations/summary", {
    query: { ...filters },
    signal,
  });

// What the API accepts when adding an organization, or changing one (an update may leave anything out).
export type OrganizationInput = {
  name: string;
  description?: string;
};

export const createOrganization = (input: OrganizationInput) =>
  api.post<Organization>("/v1/organizations", input);

export const updateOrganization = (
  id: string,
  input: Partial<OrganizationInput>,
) => api.put<Organization>(`/v1/organizations/${id}`, input);

export const deleteOrganization = (id: string) =>
  api.delete<void>(`/v1/organizations/${id}`);
