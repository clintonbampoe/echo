// Invitations as the API sends them. No React in this file.
import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";
import type { UserRole } from "@/features/account";

export type InvitationStatus = "Active" | "Expired" | "Revoked";

export type Invitation = {
  id: string;
  label?: string | null;
  allowedRole: UserRole;
  createdByName: string;
  createdAt: string;
  expiresAt: string;
  status: InvitationStatus;
  // How many people have registered with it.
  useCount: number;
};

// What creating one returns. The code is in `token` and is shown once: the server keeps only a hash.
export type CreatedInvitation = {
  id: string;
  token: string;
  allowedRole: UserRole;
  expiresAt: string;
  label?: string | null;
};

export type InvitationInput = {
  allowedRole: UserRole;
  // 1 to 30. The server uses 30 when left out.
  expiryDays?: number;
  label?: string;
};

export const listInvitations = (cursor?: string, signal?: AbortSignal) =>
  api.get<Page<Invitation>>("/v1/auth/invitations", {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

export const createInvitation = (input: InvitationInput) =>
  api.post<CreatedInvitation>("/v1/auth/invitations", input);

// Only the fields sent change. An empty label clears it; expiryDays counts from now.
export const updateInvitation = (
  id: string,
  input: { label?: string; expiryDays?: number },
) => api.put<Invitation>(`/v1/auth/invitations/${id}`, input);

export const revokeInvitation = (id: string) =>
  api.post<Invitation>(`/v1/auth/invitations/${id}/revoke`);

export const deleteInvitation = (id: string) =>
  api.delete<void>(`/v1/auth/invitations/${id}`);
