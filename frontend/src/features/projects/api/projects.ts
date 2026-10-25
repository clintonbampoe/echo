import { api, type DateWindow, type Page } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";
import { pageSize } from "@/lib/paged";

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
    query: { ...filters, cursor, pageSize: pageSize() },
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
