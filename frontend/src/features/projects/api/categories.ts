import { api } from "@/lib/api";

// What projects are grouped under, with how many each holds.
export type ProjectCategory = {
  id: number;
  name: string;
  projectCount: number;
};

export const listProjectCategories = (signal?: AbortSignal) =>
  api.get<ProjectCategory[]>("/v1/project-categories", { signal });

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
