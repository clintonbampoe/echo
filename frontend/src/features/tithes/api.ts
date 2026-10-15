// The tithe endpoints: what members give, month by month. No React in this file.
import type { PaymentMethod } from "@/features/transactions";
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

// One tithe given. forMonth is the month's name ("October"), forYear its year.
export type Tithe = {
  id: string;
  memberId: string;
  memberName: string;
  amount: number;
  forYear: number;
  forMonth: string;
  paymentMethod: PaymentMethod;
  collectionDate: string;
  description?: string | null;
  createdAt: string;
};

// What the Tithes page narrows the list by. `memberName` matches anywhere in the name; `year` and `month`
// are the period a tithe is for; `from` and `to` are the days it was collected.
export type TitheFilters = {
  memberId?: string;
  memberName?: string;
  year?: number;
  month?: string;
  paymentMethod?: PaymentMethod;
  from?: string;
  to?: string;
};

// The numbers above the list. They follow the same filters as the list.
export type TitheSummary = {
  totalCollected: number;
  uniqueTithers: number;
  // The method's name ("MobileMoney"), or nothing when no tithe matches.
  mostUsedPaymentMethod?: string | null;
  averagePerMember: number;
  window?: DateWindow | null;
};

export const listTithes = (
  filters: TitheFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Tithe>>("/v1/tithes", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

export const getTitheSummary = (filters: TitheFilters, signal?: AbortSignal) =>
  api.get<TitheSummary>("/v1/tithes/summary", {
    query: { ...filters },
    signal,
  });

export const listMemberTithes = (
  memberId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Tithe>>(`/v1/tithes/member/${memberId}`, {
    query: { cursor, pageSize: PAGE_SIZE },
    signal,
  });

// Everything a member has ever given (a request for one member has no default date range).
export const getTitheTotalEver = (memberId: string, signal?: AbortSignal) =>
  api.get<{ totalCollected: number }>("/v1/tithes/summary", {
    query: { memberId },
    signal,
  });

// One row per month in the range, oldest first. Months with nothing given come back as zero. With no range
// the server answers for the last twelve months.
export type TitheMonth = {
  year: number;
  month: number;
  totalCollected: number;
  uniqueTithers: number;
};

// Whose tithes to add up: one member, everyone whose name matches, and/or one method.
export type TitheMonthsFor = {
  memberId?: string;
  memberName?: string;
  paymentMethod?: PaymentMethod;
};

export const getTitheMonths = (
  who: TitheMonthsFor,
  range: { from?: string; to?: string },
  signal?: AbortSignal,
) =>
  api.get<TitheMonth[]>("/v1/tithes/summary/monthly", {
    query: { ...who, ...range },
    signal,
  });

// What the API accepts when recording a tithe.
export type TitheInput = {
  memberId: string;
  amount: number;
  forYear: number;
  forMonth: string;
  paymentMethod: PaymentMethod;
  collectionDate: string;
  description?: string;
};

// A tithe stays with the member it was recorded for, so an update has no member.
export type TitheUpdate = Omit<TitheInput, "memberId">;

export const createTithe = (input: TitheInput) =>
  api.post<Tithe>("/v1/tithes", input);

export const updateTithe = (id: string, input: Partial<TitheUpdate>) =>
  api.put<Tithe>(`/v1/tithes/${id}`, input);

export const deleteTithe = (id: string) => api.delete<void>(`/v1/tithes/${id}`);
