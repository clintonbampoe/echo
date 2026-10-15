// The organization endpoints: ministries, groups and teams, and who belongs to them. No React in this file.
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

// A person's part in one organization. (Not the same as what they may do in the app.)
export type OrganizationRole = "Member" | "Secretary" | "Leader";

// One person's place in one organization.
export type Membership = {
  id: string;
  organizationId: string;
  organizationName: string;
  role: OrganizationRole;
  joinedAt: string;
};

export const listPersonMemberships = (
  personId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Membership>>(`/v1/organization-members/member/${personId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

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
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
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

// One person's place in one organization, as the organization's own page lists it.
export type OrganizationMember = Membership & {
  memberId: string;
  memberName: string;
  createdAt: string;
};

export type OrganizationMemberFilters = { role?: OrganizationRole };

export const listOrganizationMembers = (
  organizationId: string,
  filters: OrganizationMemberFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<OrganizationMember>>(
    `/v1/organization-members/organization/${organizationId}`,
    {
      query: { ...filters, cursor, pageSize: PAGE_SIZE },
      signal,
    },
  );

// What the API accepts when putting a member in an organization. The server answers 409 when they are already in it.
export type OrganizationMemberInput = {
  memberId: string;
  organizationId: string;
  role: OrganizationRole;
  joinedAt: string;
};

export const addOrganizationMember = (input: OrganizationMemberInput) =>
  api.post<OrganizationMember>("/v1/organization-members", input);

export const updateOrganizationMember = (
  id: string,
  input: Partial<Pick<OrganizationMemberInput, "role" | "joinedAt">>,
) => api.put<OrganizationMember>(`/v1/organization-members/${id}`, input);

export const removeOrganizationMember = (id: string) =>
  api.delete<void>(`/v1/organization-members/${id}`);
