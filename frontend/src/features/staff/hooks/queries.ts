import { useQuery } from "@tanstack/react-query";
import type { UserRole } from "@/features/account";
import { usePagedList } from "@/lib/paged";
import * as api from "../api/staff";
import { staffKeys } from "./keys";

export const useStaffSummary = () =>
  useQuery({
    queryKey: staffKeys.summary,
    queryFn: ({ signal }) => api.getStaffSummary(signal),
  });

// The people of one role, a page at a time. Each role card reads its own list, so a role with
// thousands of people never slows the others.
export const useRoleList = (role: UserRole, query: api.StaffQuery) =>
  usePagedList(staffKeys.role(role, query), (cursor, signal) =>
    api.listStaff(role, query, cursor, signal),
  );
