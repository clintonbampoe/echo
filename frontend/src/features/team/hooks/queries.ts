import { useQuery } from "@tanstack/react-query";
import type { UserRole } from "@/features/account";
import { usePagedList } from "@/lib/paged";
import * as api from "../api/team";
import { teamKeys } from "./keys";

export const useTeamSummary = () =>
  useQuery({
    queryKey: teamKeys.summary,
    queryFn: ({ signal }) => api.getTeamSummary(signal),
  });

// The people of one role, a page at a time. Each role card reads its own list, so a role with
// thousands of people never slows the others.
export const useRoleList = (
  role: UserRole,
  status: api.TeamStatus | undefined,
) =>
  usePagedList(teamKeys.role(role, status), (cursor, signal) =>
    api.listTeam(role, status, cursor, signal),
  );
