import type { AttendanceRecord, Member, ChurchServiceType } from '../types/attendance';

// ─── Mock Database ───────────────────────────────────────────────────────────

export const mockMembers: Member[] = [
  { memberId: 1, uniqueId: 'mem-001', firstName: 'Sarah', lastName: 'Kent', dateOfBirth: '1998-05-15', phoneNumber: '0241112222', gender: 'Female', memberActivityStatus: 'Active' },
  { memberId: 2, uniqueId: 'mem-002', firstName: 'John', lastName: 'Maxwell', dateOfBirth: '1981-11-20', phoneNumber: '0243334444', gender: 'Male', memberActivityStatus: 'Active' },
  { memberId: 3, uniqueId: 'mem-003', firstName: 'John', lastName: 'Doe', dateOfBirth: '1991-03-10', phoneNumber: '0205556666', gender: 'Male', memberActivityStatus: 'Active' },
  { memberId: 4, uniqueId: 'mem-004', firstName: 'David', lastName: 'Okonjo', dateOfBirth: '2014-07-22', phoneNumber: '0277778888', gender: 'Male', memberActivityStatus: 'Active' }, // Child
  { memberId: 5, uniqueId: 'mem-005', firstName: 'Habib', lastName: 'Alhassan', dateOfBirth: '2018-01-05', phoneNumber: '0249990000', gender: 'Male', memberActivityStatus: 'Active' }, // Child
  { memberId: 6, uniqueId: 'mem-006', firstName: 'Baba', lastName: 'Tundey', dateOfBirth: '1966-09-30', phoneNumber: '0261113333', gender: 'Male', memberActivityStatus: 'Active' },
  { memberId: 7, uniqueId: 'mem-007', firstName: 'Jane', lastName: 'Smith', dateOfBirth: '2016-04-12', phoneNumber: '0242224444', gender: 'Female', memberActivityStatus: 'Active' }, // Child
  { memberId: 8, uniqueId: 'mem-008', firstName: 'Grace', lastName: 'Annan', dateOfBirth: '1994-08-01', phoneNumber: '0208889999', gender: 'Female', memberActivityStatus: 'Active' },
  { memberId: 9, uniqueId: 'mem-009', firstName: 'Emmanuel', lastName: 'Osei', dateOfBirth: '1997-02-14', phoneNumber: '0275551111', gender: 'Male', memberActivityStatus: 'Active' },
  { memberId: 10, uniqueId: 'mem-010', firstName: 'Mary', lastName: 'Smith', dateOfBirth: '1989-12-05', phoneNumber: '0246667777', gender: 'Female', memberActivityStatus: 'Active' },
  { memberId: 11, uniqueId: 'mem-011', firstName: 'Robert', lastName: 'Johnson', dateOfBirth: '1975-06-18', phoneNumber: '0209991111', gender: 'Male', memberActivityStatus: 'Active' },
  { memberId: 12, uniqueId: 'mem-012', firstName: 'Kwame', lastName: 'Nkrumah', dateOfBirth: '2013-09-21', phoneNumber: '0247772222', gender: 'Male', memberActivityStatus: 'Active' }, // Child
  { memberId: 13, uniqueId: 'mem-013', firstName: 'Ama', lastName: 'Kofi', dateOfBirth: '2017-02-28', phoneNumber: '0201115555', gender: 'Female', memberActivityStatus: 'Active' }, // Child
  { memberId: 14, uniqueId: 'mem-014', firstName: 'Patricia', lastName: 'Appiah', dateOfBirth: '1985-07-04', phoneNumber: '0273339999', gender: 'Female', memberActivityStatus: 'Active' },
  { memberId: 15, uniqueId: 'mem-015', firstName: 'Ebenezer', lastName: 'Quayson', dateOfBirth: '1990-10-10', phoneNumber: '0543210987', gender: 'Male', memberActivityStatus: 'Active' },
];

// Seed initial attendance records for Oct 23, 2026 (Evening Service)
const initialAttendanceRecords: AttendanceRecord[] = [
  { attendanceId: 1, uniqueId: 'att-001', memberId: 1, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Member', checkInTime: '15:00', description: 'Arrived early' },
  { attendanceId: 2, uniqueId: 'att-002', memberId: 2, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Member', checkInTime: '15:00', description: '' },
  { attendanceId: 3, uniqueId: 'att-003', memberId: 3, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Guest', checkInTime: '15:00', description: 'Invited by Sarah' },
  { attendanceId: 4, uniqueId: 'att-004', memberId: 4, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Member', checkInTime: '15:00', description: 'Sunday school member' },
  { attendanceId: 5, uniqueId: 'att-005', memberId: 5, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Guest', checkInTime: '15:00', description: 'Visitor child' },
  { attendanceId: 6, uniqueId: 'att-006', memberId: 6, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Visitor', checkInTime: '15:00', description: 'First-time visitor' },
  { attendanceId: 7, uniqueId: 'att-007', memberId: 7, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Member', checkInTime: '15:15', description: '' },
  { attendanceId: 8, uniqueId: 'att-008', memberId: 8, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Member', checkInTime: '15:10', description: '' },
  { attendanceId: 9, uniqueId: 'att-009', memberId: 9, forDate: '2026-10-23', churchServiceType: 'Evening', attendeeType: 'Visitor', checkInTime: '15:20', description: 'First-time visitor' },
];

// Using localStorage to make state persist across page reloads (simulating a database)
const STORAGE_KEY = 'echo_attendance_records';

const getStoredRecords = (): AttendanceRecord[] => {
  const data = localStorage.getItem(STORAGE_KEY);
  if (!data) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(initialAttendanceRecords));
    return initialAttendanceRecords;
  }
  return JSON.parse(data);
};

const setStoredRecords = (records: AttendanceRecord[]) => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(records));
};

// ─── Service Methods (Mock APIs) ─────────────────────────────────────────────

/**
 * Fetch all members/visitors in the registry
 * API equivalent: GET /api/members
 */
export const getMembers = async (): Promise<Member[]> => {
  // Simulate network delay
  return new Promise((resolve) => {
    setTimeout(() => resolve([...mockMembers]), 100);
  });
};

/**
 * Fetch attendance records filtered by date and service type
 * API equivalent: GET /api/attendance?date={date}&serviceType={serviceType}
 */
export const getAttendanceRecords = async (
  date: string,
  serviceType: ChurchServiceType
): Promise<AttendanceRecord[]> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      const allRecords = getStoredRecords();
      const filtered = allRecords.filter(
        (r) => r.forDate === date && r.churchServiceType === serviceType
      );
      resolve(filtered);
    }, 150);
  });
};

/**
 * Record or edit attendance for an individual
 * API equivalent: POST /api/attendance (or PUT /api/attendance/{id})
 */
export const saveAttendanceRecord = async (
  record: Omit<AttendanceRecord, 'attendanceId' | 'uniqueId'> & { attendanceId?: number }
): Promise<AttendanceRecord> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      const allRecords = getStoredRecords();
      
      if (record.attendanceId) {
        // Edit Mode
        const index = allRecords.findIndex((r) => r.attendanceId === record.attendanceId);
        if (index > -1) {
          const updatedRecord = {
            ...allRecords[index],
            ...record,
          } as AttendanceRecord;
          allRecords[index] = updatedRecord;
          setStoredRecords(allRecords);
          resolve(updatedRecord);
          return;
        }
      }

      // Add Mode
      const nextId = allRecords.length > 0 ? Math.max(...allRecords.map((r) => r.attendanceId)) + 1 : 1;
      const newRecord: AttendanceRecord = {
        attendanceId: nextId,
        uniqueId: `att-${String(nextId).padStart(3, '0')}`,
        memberId: record.memberId,
        forDate: record.forDate,
        churchServiceType: record.churchServiceType,
        attendeeType: record.attendeeType,
        checkInTime: record.checkInTime,
        description: record.description || '',
      };
      allRecords.push(newRecord);
      setStoredRecords(allRecords);
      resolve(newRecord);
    }, 150);
  });
};

/**
 * Delete an attendance record
 * API equivalent: DELETE /api/attendance/{id}
 */
export const deleteAttendanceRecord = async (attendanceId: number): Promise<boolean> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      const allRecords = getStoredRecords();
      const filtered = allRecords.filter((r) => r.attendanceId !== attendanceId);
      setStoredRecords(filtered);
      resolve(true);
    }, 100);
  });
};

// ─── Helper Logic ────────────────────────────────────────────────────────────

/**
 * Calculate the age based on date of birth
 */
export const calculateAge = (dateOfBirthString: string, relativeToDate: string = new Date().toISOString().split('T')[0]): number => {
  const birthDate = new Date(dateOfBirthString);
  const relativeDate = new Date(relativeToDate);
  let age = relativeDate.getFullYear() - birthDate.getFullYear();
  const m = relativeDate.getMonth() - birthDate.getMonth();
  if (m < 0 || (m === 0 && relativeDate.getDate() < birthDate.getDate())) {
    age--;
  }
  return age;
};

/**
 * Helper to determine if a member is a child (under 15 years old)
 */
export const isChild = (member: Member, relativeToDate?: string): boolean => {
  return calculateAge(member.dateOfBirth, relativeToDate) < 15;
};
