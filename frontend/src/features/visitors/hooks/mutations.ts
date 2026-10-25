import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRefreshMembers } from "@/features/members";
import type { MemberInput } from "@/features/members";
import {
  convertVisitor,
  createVisitor,
  deleteVisitor,
  updateVisitor,
  type VisitorInput,
} from "../api/visitors";
import { visitorKeys } from "./keys";

// Adding or changing a visitor changes the lists and the summary numbers, so refetch them all.
export function useRefreshVisitors() {
  const client = useQueryClient();
  return () => client.invalidateQueries({ queryKey: visitorKeys.all });
}

export function useCreateVisitor() {
  const refresh = useRefreshVisitors();
  return useMutation({ mutationFn: createVisitor, onSuccess: refresh });
}

export function useUpdateVisitor(id: string) {
  const refresh = useRefreshVisitors();
  return useMutation({
    mutationFn: (input: Partial<VisitorInput>) => updateVisitor(id, input),
    onSuccess: refresh,
  });
}

export function useDeleteVisitor() {
  const refresh = useRefreshVisitors();
  return useMutation({ mutationFn: deleteVisitor, onSuccess: refresh });
}

// Making a visitor a member changes both people lists. Settled, not just success: a 409 means someone
// else already did it, and the page should then show it.
export function useConvertVisitor(id: string) {
  const refreshVisitors = useRefreshVisitors();
  const refreshMembers = useRefreshMembers();
  return useMutation({
    mutationFn: (input: MemberInput) => convertVisitor(id, input),
    onSettled: () => Promise.all([refreshVisitors(), refreshMembers()]),
  });
}
