import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { attendanceContextsService } from "../services/attendanceContextsService";
import type {
  AttendanceContextCreatePayload,
  AttendanceContextUpdatePayload,
} from "../types/attendanceContext";

export const useAttendanceContexts = () => {
  return useQuery({
    queryKey: ["attendanceContexts"],
    queryFn: () => attendanceContextsService.getAll(),
  });
};

export const useAttendanceContext = (id?: number) => {
  return useQuery({
    queryKey: ["attendanceContexts", id],
    queryFn: () => attendanceContextsService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateAttendanceContext = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AttendanceContextCreatePayload) =>
      attendanceContextsService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["attendanceContexts", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["attendanceContexts"] });
    },
  });
};

export const useUpdateAttendanceContext = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: AttendanceContextUpdatePayload;
    }) => attendanceContextsService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["attendanceContexts", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["attendanceContexts"] });
    },
  });
};

export const useDeleteAttendanceContext = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => attendanceContextsService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["attendanceContexts", id] });
      queryClient.invalidateQueries({ queryKey: ["attendanceContexts"] });
    },
  });
};
