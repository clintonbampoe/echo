// Visitors as the API sends them. No React in this file.
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";
import type { Member, MemberInput } from "@/features/members";

export type Visitor = {
  id: string;
  name: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  emailAddress?: string | null;
  dateOfBirth?: string | null;
  notes?: string | null;
  // Set once the visitor has become a member. The member's id is the visitor's id.
  convertedToMemberPersonId?: string | null;
  convertedToMemberName?: string | null;
  convertedAt?: string | null;
  createdAt: string;
  visitCount: number;
  lastVisitedAt?: string | null;
};

// With no `converted` the list is visitors who have not become members, by the day they were added. With
// `converted: true` it is those who did, by the day they became members. No from/to means this year.
export type VisitorFilters = {
  name?: string;
  converted?: boolean;
  from?: string;
  to?: string;
};

// The numbers above the list.
export type VisitorSummary = {
  // Not yet members, in the window.
  totalVisitors: number;
  // Added in the window, whether or not they have become members since.
  newVisitors: number;
  // Not yet members, and came more than once.
  recurringVisitors: number;
  // Became members in the window.
  convertedVisitors: number;
  window?: DateWindow | null;
};

export const listVisitors = (
  filters: VisitorFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Visitor>>("/v1/visitors", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

export const getVisitor = (id: string, signal?: AbortSignal) =>
  api.get<Visitor>(`/v1/visitors/${id}`, { signal });

// The numbers describe the time range only, so the status and search do not change them.
export const getVisitorSummary = (
  filters: Pick<VisitorFilters, "from" | "to">,
  signal?: AbortSignal,
) =>
  api.get<VisitorSummary>("/v1/visitors/summary", {
    query: { ...filters },
    signal,
  });

// What the API accepts when adding a visitor, or changing one (an update may leave anything out).
export type VisitorInput = {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  emailAddress?: string;
  dateOfBirth?: string;
  notes?: string;
};

export const createVisitor = (input: VisitorInput) =>
  api.post<Visitor>("/v1/visitors", input);

export const updateVisitor = (id: string, input: Partial<VisitorInput>) =>
  api.put<Visitor>(`/v1/visitors/${id}`, input);

export const deleteVisitor = (id: string) =>
  api.delete<void>(`/v1/visitors/${id}`);

// Makes the visitor a member with the full member details. The member keeps the visitor's id and history.
// The server answers 409 when they were already converted.
export const convertVisitor = (id: string, input: MemberInput) =>
  api.post<Member>(`/v1/visitors/${id}/convert`, input);
