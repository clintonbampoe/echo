import type { PagedResponse } from "./pagination";

export type AttendeeType = "Member" | "Guest" | "Visitor" | "Child";

export interface AttendanceCreatePayload {
  attendanceContextId: number;
  memberId: string;
  attendeeType: AttendeeType;
  forDate: string; // YYYY-MM-DD
  checkInTime: string; // HH:mm:ss
  description?: string | null;
}

export type AttendanceUpdatePayload = Partial<AttendanceCreatePayload>;

export interface AttendanceRecord {
  id: string;
  attendanceContextId: number;
  attendanceContextName: string;
  attendanceTypeName: string;
  memberId: string;
  memberName: string;
  attendeeType: AttendeeType;
  forDate: string; // YYYY-MM-DD
  checkInTime: string; // HH:mm:ss
  description?: string | null;
  createdAt: string;
}

export interface AttendanceFilters {
  forDate?: string; // YYYY-MM-DD
  attendanceContextId?: number;
  memberId?: string;
  memberName?: string;
}

// UI types — move to component folder when components are refactored.
export interface AttendanceSummary {
  totalPresent: number;
  firstTimeVisitors: number;
  membersPresent: number;
  children: number;
}

export interface MarkAttendanceForm {
  memberId: string;
  attendanceContextId: number | "";
  attendeeType: AttendeeType;
  forDate: string;
  checkInTime: string;
  description: string;
}

export type { PagedResponse };
