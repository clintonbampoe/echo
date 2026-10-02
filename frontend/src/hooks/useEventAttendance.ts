import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { eventAttendanceService } from "../services/eventAttendanceService";
import type {
  EventAttendanceCreatePayload,
  EventAttendanceUpdatePayload,
} from "../types/eventAttendance";

export const useEventAttendance = (pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ["eventAttendance", pageSize, cursor],
    queryFn: () => eventAttendanceService.list(pageSize, cursor),
  });
};

export const useEventAttendanceByEvent = (
  eventId: string,
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["eventAttendance", "event", eventId, pageSize, cursor],
    queryFn: () =>
      eventAttendanceService.listByEventId(eventId, pageSize, cursor),
    enabled: !!eventId,
  });
};

export const useEventAttendanceByMember = (
  memberId: string,
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["eventAttendance", "member", memberId, pageSize, cursor],
    queryFn: () =>
      eventAttendanceService.listByMemberId(memberId, pageSize, cursor),
    enabled: !!memberId,
  });
};

export const useEventAttendanceById = (id?: string) => {
  return useQuery({
    queryKey: ["eventAttendance", id],
    queryFn: () => eventAttendanceService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateEventAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: EventAttendanceCreatePayload) =>
      eventAttendanceService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["eventAttendance", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["eventAttendance"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};

export const useUpdateEventAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: EventAttendanceUpdatePayload;
    }) => eventAttendanceService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["eventAttendance", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["eventAttendance"] });
    },
  });
};

export const useDeleteEventAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventAttendanceService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["eventAttendance", id] });
      queryClient.invalidateQueries({ queryKey: ["eventAttendance"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
};
