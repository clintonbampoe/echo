import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createCheckIn, deleteCheckIn, updateCheckIn } from "../api/checkIns";
import {
  createSessionLabel,
  deleteSessionLabel,
  updateSessionLabel,
} from "../api/sessionLabels";
import type { SessionUpdate } from "../api/sessions";
import { createSession, deleteSession, updateSession } from "../api/sessions";
import { attendanceKeys } from "./keys";

// Any change to a session, a type or a check-in changes the lists and numbers on several screens.
function useRefresh() {
  const client = useQueryClient();
  // Not awaited: the form that saved moves on at once, and the lists refresh behind it.
  return () => void client.invalidateQueries({ queryKey: attendanceKeys.all });
}

export function useCreateSession() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: createSession, onSuccess: refresh });
}

export function useUpdateSession() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & SessionUpdate) =>
      updateSession(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteSession() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: deleteSession, onSuccess: refresh });
}

export function useCheckIn(sessionId: string) {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: (personId: string) => createCheckIn(sessionId, personId),
    onSuccess: refresh,
  });
}

export function useUpdateCheckIn() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, checkedInAt }: { id: string; checkedInAt: string }) =>
      updateCheckIn(id, checkedInAt),
    onSuccess: refresh,
  });
}

export function useRemoveCheckIn() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: deleteCheckIn, onSuccess: refresh });
}

export function useCreateSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: createSessionLabel,
    onSuccess: refresh,
  });
}

export function useUpdateSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: ({ id, name }: { id: number; name: string }) =>
      updateSessionLabel(id, name),
    onSuccess: refresh,
  });
}

export function useDeleteSessionLabel() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: deleteSessionLabel,
    onSuccess: refresh,
  });
}
