import {
  useInfiniteQuery,
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { organizationsService } from "../services/organizationsService";
import type {
  OrganizationCreatePayload,
  OrganizationUpdatePayload,
} from "../types/organization";

export const useOrganizations = (pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ["organizations", pageSize, cursor],
    queryFn: () => organizationsService.list(pageSize, cursor),
  });
};

export const useOrganizationsInfinite = (pageSize: number = 24) => {
  return useInfiniteQuery({
    queryKey: ["organizations", "infinite", pageSize],
    queryFn: ({ pageParam }) => organizationsService.list(pageSize, pageParam),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? (lastPage.nextCursor ?? undefined) : undefined,
  });
};

export const useOrganizationSearch = (query: string) => {
  const trimmed = query.trim();
  return useQuery({
    queryKey: ["organizations", "search", trimmed],
    queryFn: () => organizationsService.search(trimmed),
    enabled: trimmed.length > 0,
  });
};

export const useOrganization = (id?: string) => {
  return useQuery({
    queryKey: ["organizations", id],
    queryFn: () => organizationsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: OrganizationCreatePayload) =>
      organizationsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["organizations", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
};

export const useUpdateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: OrganizationUpdatePayload;
    }) => organizationsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["organizations", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
};

export const useDeleteOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => organizationsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["organizations", id] });
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
};
