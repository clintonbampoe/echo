// The tithe endpoints: what members give, month by month. No React in this file.
import type { PaymentMethod } from "@/features/transactions";
import { api, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

// One tithe given. forMonth is the month's name ("October"), forYear its year.
export type Tithe = {
  id: string;
  amount: number;
  forYear: number;
  forMonth: string;
  paymentMethod: PaymentMethod;
  collectionDate: string;
};

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

// One row per month in the range, oldest first. Months with nothing given come back as zero.
export type TitheMonth = {
  year: number;
  month: number;
  totalCollected: number;
};

export const getTitheMonths = (
  memberId: string,
  from: string,
  to: string,
  signal?: AbortSignal,
) =>
  api.get<TitheMonth[]>("/v1/tithes/summary/monthly", {
    query: { memberId, from, to },
    signal,
  });
