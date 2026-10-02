export type TithePaymentMethod =
  "Cash" | "Cheque" | "CreditCard" | "MobileMoney";
export type TitheMonth =
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
  memberId: number;
  amount: number;
  paymentMethod: TithePaymentMethod;
  collectionDate: string; // ISO format YYYY-MM-DD
  forMonth: TitheMonth;
  forYear: number;
  description?: string | null;
}

export interface TitheUpdatePayload {
  amount?: number;
  paymentMethod?: TithePaymentMethod;
  collectionDate?: string;
  forMonth?: TitheMonth;
  forYear?: number;
  description?: string | null;
}

export interface Tithe {
  id: string;
  uniqueId: string;
  memberId: string;
  amount: number;
  paymentMethod: TithePaymentMethod;
  collectionDate: string;
  forMonth: TitheMonth;
  forYear: number;
  description: string | null;
  createdAt: string;
  // Joined member name for display
  memberName?: string;
}

export interface TitheSummary {
  totalAmount: number;
  transactionCount: number;
  lastUpdated: string;
}

export interface TitheFilters {
  year?: number;
  month?: TitheMonth;
  paymentMethod?: TithePaymentMethod;
  memberId?: string;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
