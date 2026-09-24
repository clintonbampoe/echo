import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { attendanceService } from '../services/attendanceService';
import { attendanceTypesService } from '../services/attendanceTypesService';
import { attendanceContextsService } from '../services/attendanceContextsService';
import type { AttendanceRecord, AttendanceFilters } from '../types/attendance';

export const useAttendance = (filters: AttendanceFilters = {}, pageSize: number = 50, cursor?: string) => {
  return useQuery({
    queryKey: ['attendance', filters, pageSize, cursor],
    queryFn: () => attendanceService.list(filters, pageSize, cursor),
  });
};

export const useAttendanceRecord = (id?: string) => {
  return useQuery({
    queryKey: ['attendance', id],
    queryFn: () => (id ? attendanceService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<AttendanceRecord>) => attendanceService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance'] });
    },
  });
};

export const useUpdateAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<AttendanceRecord> }) =>
      attendanceService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['attendance'] });
      queryClient.invalidateQueries({ queryKey: ['attendance', variables.id] });
    },
  });
};

export const useDeleteAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => attendanceService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance'] });
    },
  });
};

// Attendance Types
export const useAttendanceTypes = () => {
  return useQuery({
    queryKey: ['attendanceTypes'],
    queryFn: () => attendanceTypesService.getAll(),
  });
};

export const useCreateAttendanceType = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string }) => attendanceTypesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendanceTypes'] });
    },
  });
};

// Attendance Contexts
export const useAttendanceContexts = () => {
  return useQuery({
    queryKey: ['attendanceContexts'],
    queryFn: () => attendanceContextsService.getAll(),
  });
};

export const useCreateAttendanceContext = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string; attendanceTypeId: number }) =>
      attendanceContextsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendanceContexts'] });
    },
  });
};
