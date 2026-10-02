export interface Event {
  id: string;
  organizationId: string;
  organizationName: string;
  organizerId: string;
  organizerName: string;
  name: string;
  startDate: string;
  endDate: string;
  startTime?: string | null;
  endTime?: string | null;
  location?: string | null;
  capacity?: number | null;
  description?: string | null;
  createdAt: string;
  registeredCount?: number;
  attendedCount?: number;
}

export interface EventCreatePayload {
  organizationId: string;
  organizerId: string;
  name: string;
  startDate: string;
  endDate: string;
  startTime?: string | null;
  endTime?: string | null;
  location?: string | null;
  capacity?: number | null;
  description?: string | null;
}

export type EventUpdatePayload = Partial<EventCreatePayload>;

export interface EventSearchResult {
  id: string;
  name: string;
}

export interface EventFilters {
  name?: string;
  startDate?: string;
  organizationId?: string;
  organizerId?: string;
}
