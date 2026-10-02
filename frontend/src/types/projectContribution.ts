import type { PaymentMethod } from "./paymentMethod";

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

export interface ProjectContributionCreatePayload {
  projectId: string;
  amount: number;
  dateContributed: string;
  paymentMethod: PaymentMethod;
  description?: string | null;
}

export interface ProjectContributionUpdatePayload {
  amount?: number;
  dateContributed?: string;
  paymentMethod?: PaymentMethod;
  description?: string | null;
}

export interface ProjectContributionFilters {
  amount?: number;
  date?: string;
  paymentMethod?: PaymentMethod;
}
