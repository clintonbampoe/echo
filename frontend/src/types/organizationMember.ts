import type { PagedResponse } from "./pagination";

export type OrganizationMemberRole = "Member" | "Secretary" | "Leader";

export interface OrganizationMember {
  id: string;
  memberId: string;
  memberName: string;
  organizationId: string;
  organizationName: string;
  role: OrganizationMemberRole;
  joinedAt: string; // YYYY-MM-DD
  createdAt: string; // ISO datetime
}

export interface OrganizationMemberFilters {
  role?: OrganizationMemberRole;
}

export interface OrganizationMemberCreatePayload {
  memberId: string;
  organizationId: string;
  role: OrganizationMemberRole;
  joinedAt: string; // YYYY-MM-DD
}

export interface OrganizationMemberUpdatePayload {
  role?: OrganizationMemberRole | null;
  joinedAt?: string | null; // YYYY-MM-DD
}

export type { PagedResponse };
