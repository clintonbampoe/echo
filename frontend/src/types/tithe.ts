import type { PagedResponse } from "./pagination";

export type PaymentMethod =
  "Cash" | "Cheque" | "CreditCard" | "MobileMoney" | "BankTransfer";

export type MonthOfYear =
  | "January"
  | "February"
  | "March"
  | "April"
  | "May"
  | "June"
  | "July"
  | "August"
  | "September"
  | "October"
  | "November"
  | "December";

export interface TitheCreatePayload {
  memberId: string;
  amount: number;
  forYear: number;
  forMonth: MonthOfYear;
  paymentMethod: PaymentMethod;
  collectionDate: string; // YYYY-MM-DD
  description?: string | null;
}

export type TitheUpdatePayload = Partial<TitheCreatePayload>;

export interface Tithe {
  id: string;
  memberId: string;
  memberName: string;
  amount: number;
  forYear: number;
  forMonth: MonthOfYear;
  paymentMethod: PaymentMethod;
  collectionDate: string;
  description?: string | null;
  createdAt: string;
}

export interface TitheFilters {
  year?: number;
  month?: MonthOfYear;
  paymentMethod?: PaymentMethod;
  memberId?: string;
}

export type { PagedResponse };
