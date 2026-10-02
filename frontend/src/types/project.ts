export type ProjectStatus =
  "Planning" | "OnTrack" | "AtRisk" | "Complete" | "Missed";

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
}

export interface ProjectCreatePayload {
  categoryId: number;
  managerId: string;
  name: string;
  targetAmount: number;
  status: ProjectStatus;
  startDate: string;
  endDate?: string | null;
  description?: string | null;
}

export type ProjectUpdatePayload = Partial<ProjectCreatePayload>;

export interface ProjectSearchResult {
  id: string;
  name: string;
}

export interface ProjectFilters {
  name?: string;
  status?: ProjectStatus;
  categoryId?: number;
  startDate?: string;
}
