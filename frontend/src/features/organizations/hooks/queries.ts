import { useQuery } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import type { OrganizationMemberFilters } from "../api/memberships";
import {
  listOrganizationMembers,
  listPersonMemberships,
} from "../api/memberships";
import type { OrganizationFilters } from "../api/organizations";
import {
  getOrganization,
  getOrganizationSummary,
  listOrganizations,
} from "../api/organizations";
import { organizationKeys } from "./keys";

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useOrganizations = (filters: OrganizationFilters) =>
  usePagedList(organizationKeys.list(filters), (c, s) =>
    listOrganizations(filters, c, s),
  );

export const useOrganization = (id: string) =>
  useQuery({
    queryKey: organizationKeys.one(id),
    queryFn: ({ signal }) => getOrganization(id, signal),
  });

export const useOrganizationSummary = (
  filters: Pick<OrganizationFilters, "from" | "to">,
) =>
  useQuery({
    queryKey: organizationKeys.summary(filters),
    queryFn: ({ signal }) => getOrganizationSummary(filters, signal),
  });

export const useOrganizationMembers = (
  organizationId: string,
  filters: OrganizationMemberFilters,
) =>
  usePagedList(organizationKeys.members(organizationId, filters), (c, s) =>
    listOrganizationMembers(organizationId, filters, c, s),
  );

// Every organization one person belongs to.
export const usePersonMemberships = (personId: string) =>
  usePagedList(organizationKeys.person(personId), (c, s) =>
    listPersonMemberships(personId, c, s),
  );
