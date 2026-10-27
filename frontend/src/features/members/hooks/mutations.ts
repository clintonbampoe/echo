import { useMutation, useQueryClient } from "@tanstack/react-query";
import * as api from "../api/members";
import { memberKeys } from "./keys";

// Adding or changing a member changes the lists and the summary numbers, so refetch them all.
export function useRefreshMembers() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: memberKeys.all });
}

export function useCreateMember() {
  const refresh = useRefreshMembers();
  return useMutation({ mutationFn: api.createMember, onSuccess: refresh });
}

export function useDeleteMember() {
  const refresh = useRefreshMembers();
  return useMutation({ mutationFn: api.deleteMember, onSuccess: refresh });
}

export function useUpdateMember(id: string) {
  const refresh = useRefreshMembers();
  return useMutation({
    mutationFn: (input: Partial<api.MemberInput>) =>
      api.updateMember(id, input),
    onSuccess: refresh,
  });
}
