export interface Member {
  id: string;
  name: string;
  firstName: string;
  lastName: string;
  otherNames?: string;
  emailAddress?: string;
  phoneNumber: string;
  dateOfBirth: string; // ISO format YYYY-MM-DD
  joinedDate?: string;
  gender: 'Male' | 'Female' | 'Other';
  residentialAddress: string;
  city: string;
  hometown: string;
  region: string;
  gpsAddress?: string;
  maritalStatus: 'Single' | 'Married' | 'Widowed';
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: 'Active' | 'Inactive' | 'Archived' | 'Transferred';
  createdAt: string;
}

export interface PagedResponse<T> {
  data: T[];
  hasMore: boolean;
  nextCursor: string | null;
}

export interface MemberFilters {
  name?: string;
  status?: string;
  gender?: string;
  joinedDate?: string;
}
