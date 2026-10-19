// The project endpoints: fundraising and building projects. No React in this file.
import type { PaymentMethod } from "@/features/transactions";
import { api, type DateWindow, type Page } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";
import { PAGE_SIZE } from "@/lib/paged";

export type ProjectStatus =
  "Planning" | "OnTrack" | "AtRisk" | "Complete" | "Missed";

export type Project = {
  id: string;
  name: string;
  categoryId: number;
  categoryName: string;
  managerId: string;
  managerName: string;
  status: ProjectStatus;
  targetAmount: number;
  // What has been given so far, from the project's contributions.
  raisedAmount: number;
  startDate: string;
  endDate?: string | null;
  description?: string | null;
  createdAt: string;
};

// What the Projects page narrows the list by. A project is in the dates when it started on or before `to` and has
// not ended before `from`; a project with no end date stays in until it is finished with.
export type ProjectFilters = {
  name?: string;
  status?: ProjectStatus;
  categoryId?: number;
  managerId?: string;
  from: string;
  to: string;
};

export const getProject = (id: string, signal?: AbortSignal) =>
  api.get<Project>(`/v1/projects/${id}`, { signal });

export const listProjects = (
  filters: ProjectFilters,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Project>>("/v1/projects", {
    query: { ...filters, cursor, pageSize: PAGE_SIZE },
    signal,
  });

// The numbers above the list. They follow the same filters.
export type ProjectSummary = {
  totalProjects: number;
  totalRaised: number;
  totalTarget: number;
  atRiskCount: number;
  window?: DateWindow | null;
};

export const getProjectSummary = (
  filters: ProjectFilters,
  signal?: AbortSignal,
) =>
  api.get<ProjectSummary>("/v1/projects/summary", {
    query: { ...filters },
    signal,
  });

// What projects are grouped under, with how many each holds.
export type ProjectCategory = {
  id: number;
  name: string;
  projectCount: number;
};

export const listProjectCategories = (signal?: AbortSignal) =>
  api.get<ProjectCategory[]>("/v1/project-categories", { signal });

// With no dates the server answers for this year only, so a manager's list asks for all time.
export const listManagedProjects = (
  managerId: string,
  cursor?: string,
  signal?: AbortSignal,
) => listProjects({ managerId, ...ALL_TIME }, cursor, signal);

// What a new project is made of. A date is a day ("2026-03-01"); the end is left out for a project with no end yet.
export type ProjectInput = {
  categoryId: number;
  managerId: string;
  name: string;
  targetAmount: number;
  status: ProjectStatus;
  startDate: string;
  endDate?: string;
  description?: string;
};

// Saving changes sends only what is wanted changed. The server cannot take an end date away once it is set.
export type ProjectUpdate = Partial<ProjectInput>;

export const createProject = (input: ProjectInput) =>
  api.post<Project>("/v1/projects", input);

export const updateProject = (id: string, input: ProjectUpdate) =>
  api.put<Project>(`/v1/projects/${id}`, input);

// Soft delete: the project leaves the lists and totals, its contributions are kept.
export const deleteProject = (id: string) =>
  api.delete<void>(`/v1/projects/${id}`);

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

// A new category is only a name. Saving changes sends the name too: it is all there is to change.
export const createProjectCategory = (name: string) =>
  api.post<ProjectCategory>("/v1/project-categories", { name });

export const updateProjectCategory = (id: number, name: string) =>
  api.put<ProjectCategory>(`/v1/project-categories/${id}`, { name });

// A category in use cannot be deleted: the server answers with a conflict.
export const deleteProjectCategory = (id: number) =>
  api.delete<void>(`/v1/project-categories/${id}`);

// How many projects moved, and whether the old category went with them.
export type ReassignResult = { moved: number; sourceDeleted: boolean };

// Moves every project of one category to another, and deletes the first.
export const mergeProjectCategory = (id: number, targetCategoryId: number) =>
  api.post<ReassignResult>(`/v1/project-categories/${id}/reassign`, {
    targetCategoryId,
    deleteSource: true,
  });
