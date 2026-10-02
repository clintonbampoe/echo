export interface AttendanceContext {
  id: number;
  name: string;
  attendanceTypeName: string;
}

export interface AttendanceContextCreatePayload {
  name: string;
  attendanceTypeId: number;
}

export interface AttendanceContextUpdatePayload {
  name?: string;
}

export interface AttendanceContextSearchResult {
  id: number;
  name: string;
}
