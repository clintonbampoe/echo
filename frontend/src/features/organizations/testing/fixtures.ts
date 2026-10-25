import type { OrganizationMember } from "../api/memberships";
import type { Organization } from "../api/organizations";

export const anOrganization = (
  over: Partial<Organization> = {},
): Organization => ({
  id: "o1",
  name: "Church Choir",
  description: "Leads worship on Sundays.",
  createdAt: "2016-02-03T09:00:00Z",
  memberCount: 61,
  ...over,
});

export const aMembership = (
  over: Partial<OrganizationMember> = {},
): OrganizationMember => ({
  id: "om-m1",
  memberId: "m1",
  memberName: "Kofi Mensah",
  organizationId: "o1",
  organizationName: "Church Choir",
  role: "Member",
  joinedAt: "2016-01-05",
  createdAt: "2016-01-05",
  ...over,
});
