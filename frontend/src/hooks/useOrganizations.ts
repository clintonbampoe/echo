import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { organizationsService } from '../services/organizationsService';

export const useOrganizations = (pageSize: number = 50, cursor?: string) => {
  return useQuery({
    queryKey: ['organizations', pageSize, cursor],
    queryFn: () => organizationsService.list(pageSize, cursor),
  });
};

export const useOrganization = (id?: string) => {
  return useQuery({
    queryKey: ['organizations', id],
    queryFn: () => (id ? organizationsService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string; description?: string }) => organizationsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};

export const useUpdateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: { name?: string; description?: string } }) =>
      organizationsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
      queryClient.invalidateQueries({ queryKey: ['organizations', variables.id] });
    },
  });
};

export const useDeleteOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => organizationsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};
