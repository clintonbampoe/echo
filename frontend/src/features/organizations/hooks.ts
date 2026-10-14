// The only file in organizations that knows about TanStack Query.
import { usePagedList } from "@/lib/paged";
import * as api from "./api";

export const organizationKeys = {
  person: (id: string) => ["organizations", "person", id] as const,
};

// Every organization one person belongs to.
export const usePersonMemberships = (personId: string) =>
  usePagedList(organizationKeys.person(personId), (c, s) =>
    api.listPersonMemberships(personId, c, s),
  );
