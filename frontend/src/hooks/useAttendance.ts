import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { attendanceService } from "../services/attendanceService";
import type {
  AttendanceFilters,
  AttendanceCreatePayload,
  AttendanceUpdatePayload,
} from "../types/attendance";

export const useAttendance = (
  filters: AttendanceFilters = {},
  pageSize: number = 50,
  cursor?: string,
) => {
  return useQuery({
    queryKey: ["attendance", filters, pageSize, cursor],
    queryFn: () => attendanceService.list(filters, pageSize, cursor),
  });
};

export const useAttendanceRecord = (id?: string) => {
  return useQuery({
    queryKey: ["attendance", id],
    queryFn: () => attendanceService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AttendanceCreatePayload) =>
      attendanceService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["attendance", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["attendance"] });
    },
  });
};

export const useUpdateAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AttendanceUpdatePayload }) =>
      attendanceService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["attendance", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["attendance"] });
    },
  });
};

export const useDeleteAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => attendanceService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["attendance", id] });
      queryClient.invalidateQueries({ queryKey: ["attendance"] });
    },
  });
};
