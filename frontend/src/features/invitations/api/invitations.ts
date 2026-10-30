// Invitations as the API sends them. No React in this file.
import { api, type Page } from "@/lib/api";
import { pageSize } from "@/lib/paged";
import type { UserRole } from "@/features/account";

export type InvitationStatus = "Active" | "Expired" | "Revoked" | "Exhausted";

export type Invitation = {
  id: string;
  label?: string | null;
  allowedRole: UserRole;
  createdByName: string;
  createdAt: string;
  // Null when it never expires.
  expiresAt: string | null;
  status: InvitationStatus;
  // How many people have registered with it.
  useCount: number;
  // Null when any number of people may join.
  maxUses?: number | null;
  // The code, so the list can copy it. Null for invitations made before codes were kept.
  code?: string | null;
};

// What creating one returns. The code is in `token`; the list shows it again later.
export type CreatedInvitation = {
  id: string;
  token: string;
  allowedRole: UserRole;
  expiresAt: string | null;
  maxUses?: number | null;
  label?: string | null;
};

export type InvitationInput = {
  allowedRole: UserRole;
  // 1 to 365. The server uses 30 when left out. Not together with neverExpires.
  expiryDays?: number;
  neverExpires?: boolean;
  // 1 to 10000. Left out, any number of people may join.
  maxUses?: number;
  label?: string;
};

// The numbers above the list, counted by the server over every invitation.
export type InvitationSummary = {
  active: number;
  peopleJoined: number;
  expiringSoon: number;
};

export const getInvitationSummary = (signal?: AbortSignal) =>
  api.get<InvitationSummary>("/v1/auth/invitations/summary", { signal });

export const listInvitations = (cursor?: string, signal?: AbortSignal) =>
  api.get<Page<Invitation>>("/v1/auth/invitations", {
    query: { cursor, pageSize: pageSize() },
    signal,
  });

export const getInvitation = (id: string, signal?: AbortSignal) =>
  api.get<Invitation>(`/v1/auth/invitations/${id}`, { signal });

export const createInvitation = (input: InvitationInput) =>
  api.post<CreatedInvitation>("/v1/auth/invitations", input);

// Only the fields sent change. An empty label clears it; expiryDays counts from now. A limit can be
// raised but not removed, and cannot go below the people who already joined.
export type InvitationUpdate = {
  label?: string;
  expiryDays?: number;
  neverExpires?: boolean;
  maxUses?: number;
};

export const updateInvitation = (id: string, input: InvitationUpdate) =>
  api.put<Invitation>(`/v1/auth/invitations/${id}`, input);

export const revokeInvitation = (id: string) =>
  api.post<Invitation>(`/v1/auth/invitations/${id}/revoke`);

export const deleteInvitation = (id: string) =>
  api.delete<void>(`/v1/auth/invitations/${id}`);
