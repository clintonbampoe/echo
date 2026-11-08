import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { UserRole } from "@/features/account";
import * as api from "../api/staff";
import { staffKeys } from "./keys";

// Any change moves people between cards or flips a chip, and changes the bar, so refetch all of it.
function useRefresh() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: staffKeys.all });
}

export function useChangeRole() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: (v: { id: string; role: UserRole }) =>
      api.changeRole(v.id, v.role),
    onSuccess: refresh,
  });
}

export function useDisableUser() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.disableUser, onSuccess: refresh });
}

export function useEnableUser() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.enableUser, onSuccess: refresh });
}

export function useRemoveUser() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: api.removeUser, onSuccess: refresh });
}
