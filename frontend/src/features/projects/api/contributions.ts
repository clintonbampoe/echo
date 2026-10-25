import type { PaymentMethod } from "@/features/transactions";
import { api, type DateWindow, type Page } from "@/lib/api";
import { PAGE_SIZE } from "@/lib/paged";

// One gift towards a project. Contributions are not linked to a member.
export type Contribution = {
  id: string;
  projectId: string;
  projectName: string;
  amount: number;
  dateContributed: string;
  paymentMethod: PaymentMethod;
  description?: string | null;
  createdAt: string;
};

// What a project's contributions are narrowed by. A project's own list has no default window, so dates are always sent.
export type ContributionFilters = {
  projectId: string;
  paymentMethod?: PaymentMethod;
  from: string;
  to: string;
};

export const listContributions = (
  filters: ContributionFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Contribution>>("/v1/project-contributions", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// The numbers above the contributions. `mostUsedPaymentMethod` is the method's name ("MobileMoney"), by number of gifts.
export type ContributionSummary = {
  totalContributed: number;
  totalContributions: number;
  averageAmount: number;
  mostUsedPaymentMethod?: string | null;
  window?: DateWindow | null;
};

export const getContributionSummary = (
  filters: ContributionFilters,
  signal?: AbortSignal,
) =>
  api.get<ContributionSummary>("/v1/project-contributions/summary", {
    query: { ...filters },
    signal,
  });

// What a new contribution is made of. A date is a day ("2026-10-13"). The server insists the note is always sent
// (a `required` property), so no note is `null`, never left out.
export type ContributionInput = {
  projectId: string;
  amount: number;
  dateContributed: string;
  paymentMethod: PaymentMethod;
  description: string | null;
};

// Saving changes sends only what is wanted changed. A gift cannot be moved to another project: it is deleted and recorded again.
export type ContributionUpdate = Partial<Omit<ContributionInput, "projectId">>;

export const createContribution = (input: ContributionInput) =>
  api.post<Contribution>("/v1/project-contributions", input);

export const updateContribution = (id: string, input: ContributionUpdate) =>
  api.put<Contribution>(`/v1/project-contributions/${id}`, input);

export const deleteContribution = (id: string) =>
  api.delete<void>(`/v1/project-contributions/${id}`);
