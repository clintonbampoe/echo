export interface EventAttendance {
  id: string;
  memberId: string;
  memberName: string;
  eventId: string;
  eventName: string;
  checkInTime: string;
  createdAt: string;
}

export interface EventAttendanceCreatePayload {
  memberId: string;
  eventId: string;
  checkInTime: string;
}

export interface EventAttendanceUpdatePayload {
  checkInTime?: string;
}
