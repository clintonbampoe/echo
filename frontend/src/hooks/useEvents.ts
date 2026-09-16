import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { eventsService } from '../services/eventsService';
import { eventRegistrationsService } from '../services/eventRegistrationsService';
import { eventAttendanceService } from '../services/eventAttendanceService';
import { organizationsService } from '../services/organizationsService';
import type { Event, EventFilters } from '../types/event';

export const useEvents = (filters: EventFilters = {}, pageSize: number = 24, cursor?: string) => {
  return useQuery({
    queryKey: ['events', filters, pageSize, cursor],
    queryFn: () => eventsService.list(filters, pageSize, cursor),
  });
};

export const useEvent = (id?: string) => {
  return useQuery({
    queryKey: ['events', id],
    queryFn: () => (id ? eventsService.getById(id) : null),
    enabled: !!id,
  });
};

export const useCreateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Partial<Event>) => eventsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

export const useUpdateEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Event> }) => eventsService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
      queryClient.invalidateQueries({ queryKey: ['events', variables.id] });
    },
  });
};

export const useDeleteEvent = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

// Organizations / Ministries
export const useOrganizations = () => {
  return useQuery({
    queryKey: ['organizations'],
    queryFn: () => organizationsService.list(100),
  });
};

export const useCreateOrganization = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { name: string; description?: string }) => organizationsService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};

// Event Registrations
export const useEventRegistrations = (eventId?: string) => {
  return useQuery({
    queryKey: ['eventRegistrations', eventId],
    queryFn: () => (eventId ? eventRegistrationsService.listByEventId(eventId, 200) : eventRegistrationsService.list(200)),
  });
};

export const useCreateEventRegistration = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { memberId: string; eventId: string; registrationDate: string }) =>
      eventRegistrationsService.create(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['eventRegistrations'] });
      queryClient.invalidateQueries({ queryKey: ['eventRegistrations', variables.eventId] });
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

export const useDeleteEventRegistration = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventRegistrationsService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['eventRegistrations'] });
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

// Event Attendance / Check-ins
export const useEventAttendance = (eventId?: string) => {
  return useQuery({
    queryKey: ['eventAttendance', eventId],
    queryFn: () => (eventId ? eventAttendanceService.listByEventId(eventId, 200) : eventAttendanceService.list(200)),
  });
};

export const useCreateEventAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { memberId: string; eventId: string; checkInTime: string }) =>
      eventAttendanceService.create(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['eventAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['eventAttendance', variables.eventId] });
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};

export const useDeleteEventAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => eventAttendanceService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['eventAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['events'] });
    },
  });
};
