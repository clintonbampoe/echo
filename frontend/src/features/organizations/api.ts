// The organization endpoints: ministries, groups and teams, and who belongs to them. No React in this file.
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
