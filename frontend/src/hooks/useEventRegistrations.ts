import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { eventRegistrationsService } from "../services/eventRegistrationsService";
import type {
  EventRegistrationCreatePayload,
  EventRegistrationUpdatePayload,
} from "../types/eventRegistration";

export const useEventRegistrations = (
  pageSize: number = 24,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["eventRegistrations", pageSize, cursor],
    queryFn: () => eventRegistrationsService.list(pageSize, cursor),
  });
};

export const useEventRegistrationsByEvent = (
  eventId: string,
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["eventRegistrations", "event", eventId, pageSize, cursor],
    queryFn: () =>
      eventRegistrationsService.listByEventId(eventId, pageSize, cursor),
    enabled: !!eventId,
  });
};

export const useEventRegistrationsByMember = (
  memberId: string,
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["eventRegistrations", "member", memberId, pageSize, cursor],
    queryFn: () =>
      eventRegistrationsService.listByMemberId(memberId, pageSize, cursor),
    enabled: !!memberId,
  });
};

export const useEventRegistration = (id?: string) => {
  return useQuery({
    queryKey: ["eventRegistrations", id],
    queryFn: () => eventRegistrationsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateEventRegistration = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EventRegistrationCreatePayload) =>
      eventRegistrationsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["eventRegistrations", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["eventRegistrations"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};

export const useUpdateEventRegistration = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: EventRegistrationUpdatePayload;
    }) => eventRegistrationsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["eventRegistrations", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["eventRegistrations"] });
    },
  });
};

export const useDeleteEventRegistration = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventRegistrationsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["eventRegistrations", id] });
      queryClient.invalidateQueries({ queryKey: ["eventRegistrations"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};
