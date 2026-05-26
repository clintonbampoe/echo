// ─── Enums / Union Types (matching backend) ─────────────────────────────────

export type ChurchServiceType = 'General' | 'Evening' | 'ChoirPractice';

export type AttendeeType = 'Member' | 'Guest' | 'Visitor';

// ─── Entities (matching backend schemas) ─────────────────────────────────────

export interface Member {
  memberId: number;
  uniqueId: string;
  firstName: string;
  lastName: string;
  otherNames?: string;
  emailAddress?: string;
  phoneNumber: string;
  dateOfBirth: string; // YYYY-MM-DD
  joinedDate?: string | null;
  gender: string; // 'Male' | 'Female' | etc.
  residentialAddress?: string;
  city?: string;
  hometown?: string;
  region?: string;
  gpsAddress?: string;
  maritalStatus?: string;
  nextOfKin?: string;
  emergencyContactName?: string;
  emergencyContactPhoneNumber?: string;
  memberActivityStatus: string; // 'Active' | 'Inactive' | etc.
}

export interface AttendanceRecord {
  attendanceId: number;
  uniqueId: string;
  memberId: number;
  forDate: string; // YYYY-MM-DD
  churchServiceType: ChurchServiceType;
  attendeeType: AttendeeType;
  checkInTime: string; // HH:mm:ss or HH:mm
  description?: string | null;
}

// ─── Form & UI Types ─────────────────────────────────────────────────────────

export interface AttendanceSummary {
  totalPresent: number;
  firstTimeVisitors: number;
  membersPresent: number;
  children: number;
}

export interface MarkAttendanceForm {
  memberId: string; // For selection autocomplete
  status: 'Present' | 'Absent';
  roleOverride: AttendeeType;
  timeRecorded: string; // HH:mm (24h) or "hh:mm AM/PM"
  notes: string;
}
