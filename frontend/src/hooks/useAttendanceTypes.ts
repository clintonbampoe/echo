import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { attendanceTypesService } from "../services/attendanceTypesService";
import type {
  AttendanceTypeCreatePayload,
  AttendanceTypeUpdatePayload,
} from "../types/attendanceType";

export const useAttendanceTypes = () => {
  return useQuery({
    queryKey: ["attendanceTypes"],
    queryFn: () => attendanceTypesService.getAll(),
  });
};

export const useAttendanceType = (id?: number) => {
  return useQuery({
    queryKey: ["attendanceTypes", id],
    queryFn: () => attendanceTypesService.getById(id!),
    enabled: !!id,
  });
};

export const useCreateAttendanceType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: AttendanceTypeCreatePayload) =>
      attendanceTypesService.create(data),
    onSuccess: (created) => {
      queryClient.setQueryData(["attendanceTypes", created.id], created);
      queryClient.invalidateQueries({ queryKey: ["attendanceTypes"] });
    },
  });
};

export const useUpdateAttendanceType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: AttendanceTypeUpdatePayload;
    }) => attendanceTypesService.update(id, data),
    onSuccess: (updated) => {
      queryClient.setQueryData(["attendanceTypes", updated.id], updated);
      queryClient.invalidateQueries({ queryKey: ["attendanceTypes"] });
    },
  });
};

export const useDeleteAttendanceType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => attendanceTypesService.delete(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: ["attendanceTypes", id] });
      queryClient.invalidateQueries({ queryKey: ["attendanceTypes"] });
    },
  });
};
