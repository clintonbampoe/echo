import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { eventsService } from "../services/eventsService";
import type {
  EventFilters,
  EventCreatePayload,
  EventUpdatePayload,
} from "../types/event";

export const useEvents = (
  filters: EventFilters = {},
  pageSize: number = 24,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["events", filters, pageSize, cursor],
    queryFn: () => eventsService.list(filters, pageSize, cursor),
  });
};

export const useEvent = (id?: string) => {
  return useQuery({
    queryKey: ["events", id],
    queryFn: () => eventsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EventCreatePayload) => eventsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["events", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};

export const useUpdateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: EventUpdatePayload }) =>
      eventsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["events", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};

export const useDeleteEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["events", id] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};
