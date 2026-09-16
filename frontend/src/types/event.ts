export interface Event {
  id: string;
  organizationId: string;
  organizationName: string;
  organizerId: string;
  organizerName: string;
  name: string;
  startDate: string; // ISO date format YYYY-MM-DD
  endDate: string;   // ISO date format YYYY-MM-DD
  startTime?: string | null; // e.g. "09:00:00"
  endTime?: string | null;   // e.g. "11:30:00"
  location?: string | null;
  capacity?: number | null;
  description?: string | null;
  createdAt: string;
  registeredCount?: number;
  attendedCount?: number;
}

export interface Organization {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string;
}

export interface EventRegistration {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  eventName: string;
  registrationDate: string;
  createdAt: string;
}

export interface EventAttendance {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  eventName: string;
  checkInTime: string; // e.g. "09:15:00"
  createdAt: string;
}

export interface EventFilters {
  name?: string;
  startDate?: string;
  endDate?: string;
  organizationId?: string;
  organizerId?: string;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}
