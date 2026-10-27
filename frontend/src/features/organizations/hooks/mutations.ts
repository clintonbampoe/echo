import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { OrganizationMemberInput } from "../api/memberships";
import {
  addOrganizationMember,
  removeOrganizationMember,
  updateOrganizationMember,
} from "../api/memberships";
import type { OrganizationInput } from "../api/organizations";
import {
  createOrganization,
  deleteOrganization,
  updateOrganization,
} from "../api/organizations";
import { organizationKeys } from "./keys";

// Adding or changing an organization, or who is in it, changes the lists, the counts and the summary.
// One refresh covers them all, so a profile that lists someone's organizations is never left stale.
export function useRefreshOrganizations() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: organizationKeys.all });
}

export function useCreateOrganization() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: createOrganization,
    onSuccess: refresh,
  });
}

export function useUpdateOrganization(id: string) {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: (input: Partial<OrganizationInput>) =>
      updateOrganization(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteOrganization() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: deleteOrganization,
    onSuccess: refresh,
  });
}

export function useAddOrganizationMember() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: addOrganizationMember,
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
      Pick<OrganizationMemberInput, "role" | "joinedAt">
    >) => updateOrganizationMember(id, input),
    onSuccess: refresh,
  });
}

export function useRemoveOrganizationMember() {
  const refresh = useRefreshOrganizations();
  return useMutation({
    mutationFn: removeOrganizationMember,
    onSuccess: refresh,
  });
}
