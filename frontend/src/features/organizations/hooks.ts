// The only file in organizations that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const organizationKeys = {
  all: ["organizations"] as const,
  list: (filters: api.OrganizationFilters) =>
    ["organizations", "list", filters] as const,
  summary: (filters: Pick<api.OrganizationFilters, "from" | "to">) =>
    ["organizations", "summary", filters] as const,
  one: (id: string) => ["organizations", "one", id] as const,
  members: (id: string, filters: api.OrganizationMemberFilters) =>
    ["organizations", "members", id, filters] as const,
  person: (id: string) => ["organizations", "person", id] as const,
};

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useOrganizations = (filters: api.OrganizationFilters) =>
  usePagedList(organizationKeys.list(filters), (c, s) =>
    api.listOrganizations(filters, c, s),
  );

export const useOrganization = (id: string) =>
  useQuery({
    queryKey: organizationKeys.one(id),
    queryFn: ({ signal }) => api.getOrganization(id, signal),
  });

export const useOrganizationSummary = (
  filters: Pick<api.OrganizationFilters, "from" | "to">,
) =>
  useQuery({
    queryKey: organizationKeys.summary(filters),
    queryFn: ({ signal }) => api.getOrganizationSummary(filters, signal),
  });

export const useOrganizationMembers = (
  organizationId: string,
  filters: api.OrganizationMemberFilters,
) =>
  usePagedList(organizationKeys.members(organizationId, filters), (c, s) =>
    api.listOrganizationMembers(organizationId, filters, c, s),
  );

// Every organization one person belongs to.
export const usePersonMemberships = (personId: string) =>
  usePagedList(organizationKeys.person(personId), (c, s) =>
    api.listPersonMemberships(personId, c, s),
  );

// Adding or changing an organization, or who is in it, changes the lists, the counts and the summary.
// One refresh covers them all, so a profile that lists someone's organizations is never left stale.
function useRefreshOrganizations() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: organizationKeys.all });
}

export function useCreateOrganization() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: api.createOrganization,
    onSuccess: refresh,
  });
}

export function useUpdateOrganization(id: string) {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: (input: Partial<api.OrganizationInput>) =>
      api.updateOrganization(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteOrganization() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: api.deleteOrganization,
    onSuccess: refresh,
  });
}

export function useAddOrganizationMember() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: api.addOrganizationMember,
    onSuccess: refresh,
  });
}

// Changes a member's role or the day they joined. Takes the membership's id with the change.
export function useUpdateOrganizationMember() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: ({
      id,
      ...input
    }: { id: string } & Partial<
      Pick<api.OrganizationMemberInput, "role" | "joinedAt">
    >) => api.updateOrganizationMember(id, input),
    onSuccess: refresh,
  });
}

export function useRemoveOrganizationMember() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: api.removeOrganizationMember,
    onSuccess: refresh,
  });
}
