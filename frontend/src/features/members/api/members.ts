// Members as the API sends them. No React in this file.
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

export type Gender = "Male" | "Female" | "Other";
export type MaritalStatus = "Single" | "Married" | "Widowed";
export type MemberStatus = "Active" | "Inactive" | "Archived" | "Transferred";

export type Member = {
  id: string;
  name: string;
  firstName: string;
  lastName: string;
  otherNames?: string | null;
  emailAddress?: string | null;
  phoneNumber: string;
  dateOfBirth: string;
  joinedDate?: string | null;
  gender: Gender;
  residentialAddress: string;
  city: string;
  hometown: string;
  region: string;
  gpsAddress?: string | null;
  maritalStatus: MaritalStatus;
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: MemberStatus;
  createdAt: string;
};

// Every filter is optional. With no from/to the server uses this year (by joined date).
export type MemberFilters = {
  name?: string;
  status?: MemberStatus;
  gender?: Gender;
  region?: string;
  maritalStatus?: MaritalStatus;
  from?: string;
  to?: string;
};

// The numbers above the list. They follow the same filters as the list.
export type MemberSummary = {
  totalMembers: number;
  activeMembers: number;
  maleCount: number;
  femaleCount: number;
  averageAge: number;
  window?: DateWindow | null;
};

export const listMembers = (
  filters: MemberFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Member>>("/v1/members", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// A person found by the quick search: just enough to pick them.
export type MemberMatch = { id: string; name: string; phoneNumber: string };

// Best matches first. The server sends a plain array (no paging) and limits how often it can be called.
export const searchMembers = (q: string, signal?: AbortSignal) =>
  api.get<MemberMatch[]>("/v1/members/search", { query: { q }, signal });

export const getMember = (id: string, signal?: AbortSignal) =>
  api.get<Member>(`/v1/members/${id}`, { signal });

export const getMemberSummary = (
  filters: MemberFilters,
  signal?: AbortSignal,
) =>
  api.get<MemberSummary>("/v1/members/summary", {
    query: { ...filters },
    signal,
  });

// What the API accepts when creating a member, or changing one (an update may leave anything out).
export type MemberInput = {
  firstName: string;
  lastName: string;
  otherNames?: string;
  emailAddress?: string;
  phoneNumber: string;
  dateOfBirth: string;
  joinedDate?: string;
  gender: Gender;
  residentialAddress: string;
  city: string;
  hometown: string;
  region: string;
  gpsAddress?: string;
  maritalStatus: MaritalStatus;
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: MemberStatus;
};

export const createMember = (input: MemberInput) =>
  api.post<Member>("/v1/members", input);

export const deleteMember = (id: string) =>
  api.delete<void>(`/v1/members/${id}`);

export const updateMember = (id: string, input: Partial<MemberInput>) =>
  api.put<Member>(`/v1/members/${id}`, input);
