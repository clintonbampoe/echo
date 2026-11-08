// The people of a congregation as the API sends them. No React in this file.
import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";
import type { UserRole } from "@/features/account";

export type StaffStatus = "Active" | "Disabled" | "Unverified";

export type StaffUser = {
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
export type StaffSummary = {
  total: number;
  disabled: number;
  roles: RoleCount[];
};

export const getStaffSummary = (signal?: AbortSignal) =>
  api.get<StaffSummary>("/v1/users/summary", { signal });

// What narrows a role's list: the account's status, text in the name or email, and how many to load at a time
// (a card shows a few; the role's own page uses the person's "Rows per page").
export type StaffQuery = { status?: StaffStatus; q?: string; size?: number };

export const listStaff = (
  role: UserRole,
  { status, q, size }: StaffQuery,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<StaffUser>>("/v1/users", {
    query: {
      role,
      status,
      q: q || undefined,
      cursor,
      pageSize: size ?? pageSize(),
    },
    signal,
  });

export const changeRole = (id: string, role: UserRole) =>
  api.put<StaffUser>(`/v1/users/${id}`, { role });

export const disableUser = (id: string) =>
  api.post<StaffUser>(`/v1/users/${id}/disable`);

export const enableUser = (id: string) =>
  api.post<StaffUser>(`/v1/users/${id}/enable`);

export const removeUser = (id: string) => api.delete<void>(`/v1/users/${id}`);
