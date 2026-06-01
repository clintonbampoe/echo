export type TitheRecord = {
  titheId: number;
  uniqueId: string;
  memberId: number;
  amount: number;
  paymentMethod: 'Cash' | 'Cheque' | 'CreditCard' | 'MobileMoney';
  collectionDate: string;
  forMonth: 'January' | 'February' | 'March' | 'April' | 'May' | 'June' | 'July' | 'August' | 'September' | 'October' | 'November' | 'December';
  forYear: number;
  description: string | null;
  // Included from joined Member table for convenience
  memberName?: string; 
};

export type TitheSummary = {
  totalAmount: number;
  transactionCount: number;
  lastUpdated: string;
};
