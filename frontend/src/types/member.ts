import type { PagedResponse } from "./pagination";

export type MemberGender = "Male" | "Female" | "Other";

export type MemberMaritalStatus = "Single" | "Married" | "Widowed";

export type MemberStatus = "Active" | "Inactive" | "Archived" | "Transferred";

export type MemberRegion =
  | "Ahafo"
  | "Ashanti"
  | "Bono"
  | "BonoEast"
  | "Central"
  | "Eastern"
  | "GreaterAccra"
  | "NorthEast"
  | "Northern"
  | "Oti"
  | "Savannah"
  | "UpperEast"
  | "UpperWest"
  | "Volta"
  | "Western"
  | "WesternNorth";

export interface MemberCreatePayload {
  firstName: string;
  lastName: string;
  otherNames?: string | null;
  emailAddress?: string | null;
  phoneNumber: string;
  dateOfBirth: string; // YYYY-MM-DD — required
  joinedDate?: string | null; // YYYY-MM-DD
  gender: MemberGender;
  residentialAddress: string;
  city: string;
  hometown: string;
  region: MemberRegion;
  gpsAddress?: string | null;
  maritalStatus: MemberMaritalStatus;
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: MemberStatus;
}

export type MemberUpdatePayload = Partial<MemberCreatePayload>;

export interface Member {
  id: string;
  name: string;
  firstName: string;
  lastName: string;
  otherNames?: string | null;
  emailAddress?: string | null;
  phoneNumber: string;
  dateOfBirth: string;
  joinedDate?: string | null;
  gender: MemberGender;
  residentialAddress: string;
  city: string;
  hometown: string;
  region: MemberRegion;
  gpsAddress?: string | null;
  maritalStatus: MemberMaritalStatus;
  nextOfKin: string;
  emergencyContactName: string;
  emergencyContactPhoneNumber: string;
  status: MemberStatus;
  createdAt: string;
}

export interface MemberSearchResult {
  id: string;
  name: string;
  phoneNumber: string;
}

export interface MemberFilters {
  name?: string;
  status?: MemberStatus;
  gender?: MemberGender;
  joinedDate?: string; // YYYY-MM-DD
}

export type { PagedResponse };
