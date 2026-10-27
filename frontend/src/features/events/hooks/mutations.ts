import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  checkInMember,
  deleteCheckIn,
  updateCheckInTime,
} from "../api/checkIns";
import type { EventInput } from "../api/events";
import { createEvent, deleteEvent, updateEvent } from "../api/events";
import { deleteRegistration, registerMember } from "../api/registrations";
import { eventKeys } from "./keys";

// Adding, changing or removing an event changes the lists, the counts, the summary, and every page that shows an
// event of someone's. One refresh covers them all.
function useRefreshEvents() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: eventKeys.all });
}

export function useCreateEvent() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: createEvent, onSuccess: refresh });
}

export function useUpdateEvent(id: string) {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: (input: Partial<EventInput>) => updateEvent(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteEvent() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: deleteEvent, onSuccess: refresh });
}

export function useRemoveRegistration() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: deleteRegistration,
    onSuccess: refresh,
  });
}

export function useRemoveCheckIn() {
  const refresh = useRefreshEvents();
  return useMutation({ mutationFn: deleteCheckIn, onSuccess: refresh });
}

export function useUpdateCheckInTime() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: ({ id, time }: { id: string; time: string }) =>
      updateCheckInTime(id, time),
    onSuccess: refresh,
  });
}

// Recording a person at the door is quick and repeated, so the lists refresh behind it instead of holding it up. Settled,
// not just success: a refusal (the event filled up, or they were already recorded) means someone else changed it.
export function useRegisterMember() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: registerMember,
    onSettled: () => void refresh(),
  });
}

export function useCheckInMember() {
  const refresh = useRefreshEvents();
  return useMutation({
    mutationFn: checkInMember,
    onSettled: () => void refresh(),
  });
}
