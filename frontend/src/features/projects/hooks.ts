// The only file in projects that knows about TanStack Query.
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const projectKeys = {
  managed: (id: string) => ["projects", "managed", id] as const,
};

// Every project one person manages.
export const useManagedProjects = (managerId: string) =>
  usePagedList(projectKeys.managed(managerId), (c, s) =>
    api.listManagedProjects(managerId, c, s),
  );
