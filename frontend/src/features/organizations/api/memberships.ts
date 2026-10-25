import { api, type Page } from "@/lib/api";
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

// One person's place in one organization, as the organization's own page lists it.
export type OrganizationMember = Membership & {
  memberId: string;
  memberName: string;
  createdAt: string;
};

// `name` matches anywhere in the person's name, in any case.
export type OrganizationMemberFilters = {
  role?: OrganizationRole;
  name?: string;
};

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
