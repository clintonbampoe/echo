import { useQuery } from "@tanstack/react-query";
import { ALL_TIME } from "@/lib/dates";
import { usePagedList } from "@/lib/paged";
import * as api from "../api/members";
import { memberKeys } from "./keys";

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useMembers = (filters: api.MemberFilters) =>
  usePagedList(memberKeys.list(filters), (c, s) =>
    api.listMembers(filters, c, s),
  );

export const useMember = (id: string) =>
  useQuery({
    queryKey: memberKeys.one(id),
    queryFn: ({ signal }) => api.getMember(id, signal),
  });

// People whose name or phone matches `q`. Waits for at least two letters; the caller debounces typing.
export const useMemberSearch = (q: string) =>
  useQuery({
    queryKey: memberKeys.search(q),
    queryFn: ({ signal }) => api.searchMembers(q, signal),
    enabled: q.length >= 2,
  });

// `enabled: false` leaves the request unsent, for a number only some screens need.
export const useMemberSummary = (filters: api.MemberFilters, enabled = true) =>
  useQuery({
    queryKey: memberKeys.summary(filters),
    queryFn: ({ signal }) => api.getMemberSummary(filters, signal),
    enabled,
  });

// How many members are active today, whenever they joined (a summary with no dates counts only this year's joiners).
export const useActiveMemberCount = () =>
  useMemberSummary(ALL_TIME).data?.activeMembers;
