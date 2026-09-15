import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { organizationsService } from '../services/organizationsService';
import { Organization } from '../types/organization';

export const useOrganizations = (pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ['organizations', pageSize, cursor],
    queryFn: () => organizationsService.list(pageSize, cursor),
  });
};

export const useOrganization = (id: string) => {
  return useQuery({
    queryKey: ['organizations', id],
    queryFn: () => organizationsService.getById(id),
    enabled: !!id,
  });
};

export const useCreateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Organization>) => organizationsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};

export const useUpdateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Organization> }) => organizationsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
      queryClient.invalidateQueries({ queryKey: ['organizations', variables.id] });
    },
  });
};
