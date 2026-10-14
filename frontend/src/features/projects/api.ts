// The project endpoints: fundraising and building projects. No React in this file.
import { api, type Page } from "@/lib/api";
import { ALL_TIME } from "@/lib/dates";
import { PAGE_SIZE } from "@/lib/paged";

export type ProjectStatus =
  "Planning" | "OnTrack" | "AtRisk" | "Complete" | "Missed";

export type Project = {
  id: string;
  name: string;
  status: ProjectStatus;
  targetAmount: number;
  raisedAmount: number;
};

// With no dates the server answers for this year only, so a manager's list asks for all time.
export const listManagedProjects = (
  managerId: string,
  cursor?: string,
  signal?: AbortSignal,
) =>
  api.get<Page<Project>>("/v1/projects", {
    query: { managerId, ...ALL_TIME, cursor, pageSize: PAGE_SIZE },
    signal,
  });
