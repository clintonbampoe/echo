import type { PagedResponse } from "./pagination";

export interface OrganizationCreatePayload {
  name: string;
  description?: string | null;
}

export interface OrganizationUpdatePayload {
  name?: string;
  description?: string | null;
}

export interface Organization {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string; // ISO datetime
}

export interface OrganizationSearchResult {
  id: string;
  name: string;
}

export type { PagedResponse };
