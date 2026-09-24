export type AttendeeType = 'Member' | 'Guest' | 'Visitor' | 'Child';

export interface AttendanceRecord {
  id: string;
  attendanceContextId: number;
  attendanceContextName: string;
  attendanceTypeName: string;
  memberId: string;
  memberName: string;
  attendeeType: AttendeeType;
  forDate: string; // ISO format YYYY-MM-DD
  checkInTime: string; // HH:mm:ss or HH:mm
  description?: string | null;
  createdAt: string;
}

export interface AttendanceType {
  id: number;
  name: string;
}

export interface AttendanceContext {
  id: number;
  name: string;
  attendanceTypeName?: string;
  attendanceTypeId?: number;
}

export interface AttendanceFilters {
  forDate?: string;
  attendanceContextId?: number;
  memberId?: string;
  memberName?: string;
}

export interface AttendanceSummary {
  totalPresent: number;
  firstTimeVisitors: number;
  membersPresent: number;
  children: number;
}

export interface MarkAttendanceForm {
  memberId: string;
  attendanceContextId: number | '';
  attendeeType: AttendeeType;
  forDate: string;
  checkInTime: string;
  description: string;
}

export interface PagedResponse<T> {
  hasMore: boolean;
  nextCursor: string | null;
  data: T[];
}

