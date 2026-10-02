export interface AttendanceType {
  id: number;
  name: string;
}

export interface AttendanceTypeCreatePayload {
  name: string;
}

export interface AttendanceTypeUpdatePayload {
  name?: string;
}

export interface AttendanceTypeSearchResult {
  id: number;
  name: string;
}
