import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { membersService } from '../services/membersService';
import { Member, MemberFilters } from '../types/member';

export const useMembers = (filters: MemberFilters = {}, pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ['members', filters, pageSize, cursor],
    queryFn: () => membersService.list(filters, pageSize, cursor),
  });
};

export const useMember = (id: string) => {
  return useQuery({
    queryKey: ['members', id],
    queryFn: () => membersService.getById(id),
    enabled: !!id,
  });
};

export const useCreateMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Member>) => membersService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members'] });
    },
  });
};

export const useUpdateMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Member> }) => membersService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['members'] });
      queryClient.invalidateQueries({ queryKey: ['members', variables.id] });
    },
  });
};

export const useDeleteMember = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => membersService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['members'] });
    },
  });
};
