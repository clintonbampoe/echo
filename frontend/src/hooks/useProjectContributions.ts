import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectContributionsService } from '../services/projectContributionsService';
import type { ProjectContribution, ProjectContributionFilters } from '../types/project';

export const useProjectContributions = (
  filters: ProjectContributionFilters = {},
  pageSize: number = 24,
  cursor?: string
) => {
  return useQuery({
    queryKey: ['projectContributions', filters, pageSize, cursor],
    queryFn: () => projectContributionsService.list(filters, pageSize, cursor),
  });
};

export const useCreateProjectContribution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<ProjectContribution>) => projectContributionsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectContributions'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

export const useDeleteProjectContribution = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projectContributionsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectContributions'] });
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};
