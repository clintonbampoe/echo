import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { membersService } from "../services/membersService";
import type {
  MemberCreatePayload,
  MemberUpdatePayload,
  MemberFilters,
} from "../types/member";

export const useMembers = (
  filters: MemberFilters = {},
  pageSize: number = 24,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["members", filters, pageSize, cursor],
    queryFn: () => membersService.list(filters, pageSize, cursor),
  });
};

export const useMember = (id?: string) => {
  return useQuery({
    queryKey: ["members", id],
    queryFn: () => membersService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: MemberCreatePayload) => membersService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["members", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["members"] });
    },
  });
};

export const useUpdateMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: MemberUpdatePayload }) =>
      membersService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["members", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["members"] });
    },
  });
};

export const useDeleteMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => membersService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["members", id] });
      queryClient.invalidateQueries({ queryKey: ["members"] });
    },
  });
};
