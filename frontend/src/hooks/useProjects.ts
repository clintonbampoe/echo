import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsService } from '../services/projectsService';
import { projectCategoriesService } from '../services/projectCategoriesService';
import type { Project, ProjectFilters } from '../types/project';

export const useProjects = (filters: ProjectFilters = {}, pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ['projects', filters, pageSize, cursor],
    queryFn: () => projectsService.list(filters, pageSize, cursor),
  });
};

export const useProject = (id?: string) => {
  return useQuery({
    queryKey: ['projects', id],
    queryFn: () => (id ? projectsService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Project>) => projectsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

export const useUpdateProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Project> }) => projectsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
      queryClient.invalidateQueries({ queryKey: ['projects', variables.id] });
    },
  });
};

export const useDeleteProject = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projectsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] });
    },
  });
};

export const useProjectCategories = () => {
  return useQuery({
    queryKey: ['projectCategories'],
    queryFn: () => projectCategoriesService.getAll(),
  });
};

export const useCreateProjectCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string }) => projectCategoriesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projectCategories'] });
    },
  });
};
