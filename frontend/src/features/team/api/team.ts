// The people of a congregation as the API sends them. No React in this file.
import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";
import type { UserRole } from "@/features/account";

export type TeamStatus = "Active" | "Disabled" | "Unverified";

export type TeamUser = {
  id: string;
  name: string;
  emailAddress: string;
  role: UserRole;
  // Null until the person verifies their email.
  verifiedAt: string | null;
  // Null while the account can sign in.
  disabledAt: string | null;
  createdAt: string;
};

export type RoleCount = { role: UserRole; count: number; disabled: number };

// The head counts over the whole congregation, one entry for every role (even one nobody holds).
export type TeamSummary = {
  total: number;
  disabled: number;
  roles: RoleCount[];
};

export const getTeamSummary = (signal?: AbortSignal) =>
  api.get<TeamSummary>("/v1/users/summary", { signal });

export const listTeam = (
  role: UserRole,
  status: TeamStatus | undefined,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<TeamUser>>("/v1/users", {
    query: { role, status, cursor, pageSize: pageSize() },
    signal,
  });

export const changeRole = (id: string, role: UserRole) =>
  api.put<TeamUser>(`/v1/users/${id}`, { role });

export const disableUser = (id: string) =>
  api.post<TeamUser>(`/v1/users/${id}/disable`);

export const enableUser = (id: string) =>
  api.post<TeamUser>(`/v1/users/${id}/enable`);

export const removeUser = (id: string) => api.delete<void>(`/v1/users/${id}`);
