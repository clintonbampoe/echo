export type PaymentMethod = 'Cash' | 'Cheque' | 'CreditCard' | 'MobileMoney' | 'BankTransfer';

export type TransactionType = 'Income' | 'Expense';

export type MonthOfYear =
  | 'January'
  | 'February'
  | 'March'
  | 'April'
  | 'May'
  | 'June'
  | 'July'
  | 'August'
  | 'September'
  | 'October'
  | 'November'
  | 'December';

export interface TransactionCategory {
  id: number;
  name: string;
  categoryType: TransactionType;
}

export interface Transaction {
  id: string;
  categoryId: number;
  categoryName: string;
  transactionType: TransactionType;
  transactionDate: string; // ISO format YYYY-MM-DD
  amount: number;
  description?: string | null;
  createdAt: string;
}

export interface Tithe {
  id: string;
  memberId: string;
  memberName: string;
  amount: number;
  forYear: number;
  forMonth: MonthOfYear;
  paymentMethod: PaymentMethod;
  collectionDate: string; // ISO format YYYY-MM-DD
  description?: string | null;
  createdAt: string;
}

export interface CategoryStream {
  category: {
    id: number;
    name: string;
    categoryType: TransactionType;
  };
  totalAmount: number;
  percentOfTotal: number;
  transactionCount: number;
}

export interface RecordTransactionForm {
  transactionType: TransactionType;
  amount: string;
  categoryId: number | '';
  transactionDate: string;
  description: string;
}

export interface CategoryForm {
  name: string;
  categoryType: TransactionType;
}

export interface TransactionFilters {
  date?: string;
  transactionType?: TransactionType;
  categoryId?: number;
}

export interface TitheFilters {
  year?: number;
  month?: MonthOfYear;
  paymentMethod?: PaymentMethod;
  memberId?: string;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
