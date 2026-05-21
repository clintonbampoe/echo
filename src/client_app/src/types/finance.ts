// ─── Enums (matching backend) ────────────────────────────────────────────────

export type TransactionType = 'Income' | 'Expenditure';

export type PaymentMethod = 'Cash' | 'Cheque' | 'CreditCard' | 'MobileMoney';

export type RecurrenceType =
  | 'one_time'
  | 'weekly'
  | 'bi_weekly'
  | 'monthly'
  | 'quarterly'
  | 'annually';

// ─── Entities (matching backend schema) ──────────────────────────────────────

export interface TransactionCategory {
  categoryId: number;
  uniqueId: string;
  name: string;
  categoryType: TransactionType;
}

export interface FinancialTransaction {
  transactionId: number;
  uniqueId: string;
  categoryId: number | null;
  transactionType: TransactionType;
  transactionDate: string; // ISO date string (YYYY-MM-DD)
  amount: number;
  description: string | null;
  // Frontend-only fields (not yet in backend entity):
  paymentMethod?: PaymentMethod;
  isRecurring?: boolean;
  recurrenceType?: RecurrenceType;
}

// ─── Computed / Display types ────────────────────────────────────────────────

export interface CategoryStream {
  category: TransactionCategory;
  totalAmount: number;
  percentOfTotal: number;
  isRecurring: boolean;
  recurrenceLabel: string;
  transactionCount: number;
}

export interface FinancialSummary {
  totalMonthlyIncome: number;
  totalExpenditure: number;
  netBalance: number;
  incomeTransactionCount: number;
  expenditureTransactionCount: number;
}

// ─── Modal / Form types ──────────────────────────────────────────────────────

export interface RecordTransactionForm {
  transactionType: TransactionType;
  amount: string;
  categoryId: string;
  transactionDate: string;
  paymentMethod: PaymentMethod;
  description: string;
}

export type DateRangeOption = 'last_30' | 'last_90' | 'this_year' | 'custom';
export type ReportType = 'full' | 'income_only' | 'expenditure_only';
export type ExportFormat = 'pdf' | 'excel' | 'csv';

export interface ExportReportForm {
  dateRange: DateRangeOption;
  reportType: ReportType;
  exportFormat: ExportFormat;
}

export interface CategoryForm {
  name: string;
  categoryType: TransactionType;
}
