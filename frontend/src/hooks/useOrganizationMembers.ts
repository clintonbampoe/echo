import {
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { organizationMembersService } from "../services/organizationMembersService";
import type {
  OrganizationMemberFilters,
  OrganizationMemberCreatePayload,
  OrganizationMemberUpdatePayload,
} from "../types/organizationMember";

export const useOrganizationMembersInfinite = (
  organizationId?: string,
  filters: OrganizationMemberFilters = {},
  pageSize: number = 24,
) => {
  return useInfiniteQuery({
    queryKey: ["organizationMembers", organizationId, filters, pageSize],
    queryFn: ({ pageParam }) =>
      organizationMembersService.listByOrganization(
        organizationId!,
        filters,
        pageSize,
        pageParam,
      ),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? (lastPage.nextCursor ?? undefined) : undefined,
    enabled: !!organizationId,
  });
};

export const useCreateOrganizationMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: OrganizationMemberCreatePayload) =>
      organizationMembersService.create(data),
    onSuccess: (created) => {
      queryClient.invalidateQueries({
        queryKey: ["organizationMembers", created.organizationId],
      });
    },
  });
};

export const useUpdateOrganizationMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: OrganizationMemberUpdatePayload;
    }) => organizationMembersService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizationMembers"] });
    },
  });
};

export const useDeleteOrganizationMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => organizationMembersService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizationMembers"] });
    },
  });
};
