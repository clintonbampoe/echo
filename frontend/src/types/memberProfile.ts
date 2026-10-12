export type MemberActivitySection =
  | "attendance"
  | "tithes"
  | "organizations"
  | "events-attended"
  | "events-registered"
  | "projects-managed"
  | "events-organized";

/** Mirrors the backend AttendanceResponseDto (church service attendance). */
export interface MemberAttendanceRecord {
  id: string;
  attendanceTypeId: number;
  attendanceTypeName: string;
  personId: string;
  personName: string;
  personKind: "Member" | "Visitor";
  date: string; // YYYY-MM-DD
  checkInTime: string; // HH:mm:ss
  notes?: string | null;
  createdAt: string;
}
