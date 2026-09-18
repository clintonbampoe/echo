export type ProjectStatus = 'Planning' | 'OnTrack' | 'AtRisk' | 'Complete' | 'Missed';

export type PaymentMethod = 'Cash' | 'Cheque' | 'CreditCard' | 'MobileMoney' | 'BankTransfer';

export interface Project {
  id: string;
  categoryId: number;
  categoryName: string;
  managerId: string;
  managerName: string;
  name: string;
  targetAmount: number;
  status: ProjectStatus;
  startDate: string;
  endDate?: string | null;
  description?: string | null;
  createdAt: string;
  // Computed on frontend or combined with contributions
  raisedAmount?: number;
}

export interface ProjectCategory {
  id: number;
  name: string;
}

export interface ProjectContribution {
  id: string;
  projectId: string;
  projectName: string;
  amount: number;
  dateContributed: string;
  paymentMethod: PaymentMethod;
  description: string | null;
  createdAt: string;
}

export interface ProjectFilters {
  name?: string;
  status?: ProjectStatus;
  categoryId?: number;
  startDate?: string;
}

export interface ProjectContributionFilters {
  amount?: number;
  date?: string;
  paymentMethod?: PaymentMethod;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
