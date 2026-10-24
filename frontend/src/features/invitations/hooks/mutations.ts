import { useMutation, useQueryClient } from "@tanstack/react-query";
import * as api from "../api/invitations";
import { invitationKeys } from "./keys";

// Any change to an invitation changes the list and the figures above it, so refetch them.
function useRefresh() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: invitationKeys.all });
}

export function useCreateInvitation() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.createInvitation, onSuccess: refresh });
}

export function useUpdateInvitation(id: string) {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: (input: { label?: string; expiryDays?: number }) =>
      api.updateInvitation(id, input),
    onSuccess: refresh,
  });
}

export function useRevokeInvitation() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.revokeInvitation, onSuccess: refresh });
}

export function useDeleteInvitation() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.deleteInvitation, onSuccess: refresh });
}
