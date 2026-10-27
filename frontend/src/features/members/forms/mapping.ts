// Moves a member between the shapes it has: what the API sends, what a form holds, what the API accepts.
// No React in this file.
import type { Member, MemberInput } from "../api/members";
import type { MemberValues } from "./schema";

// A blank form. Choices that always have a value start on the most common one.
export const emptyMember: MemberValues = {
  firstName: "",
  lastName: "",
  otherNames: "",
  phoneNumber: "",
  emailAddress: "",
  dateOfBirth: "",
  joinedDate: "",
  gender: "Male",
  maritalStatus: "Single",
  status: "Active",
  residentialAddress: "",
  city: "",
  hometown: "",
  region: "GreaterAccra",
  gpsAddress: "",
  nextOfKin: "",
  emergencyContactName: "",
  emergencyContactPhoneNumber: "",
};

// The form holds text for every field; the API wants optional ones left out when empty.
export function toInput(v: MemberValues): MemberInput {
  return {
    ...v,
    otherNames: v.otherNames || undefined,
    emailAddress: v.emailAddress || undefined,
    joinedDate: v.joinedDate || undefined,
    gpsAddress: v.gpsAddress || undefined,
  };
}

// Fills the edit form from a member the server sent. Missing optional values become empty text.
export function fromMember(m: Member): MemberValues {
  return {
    firstName: m.firstName,
    lastName: m.lastName,
    otherNames: m.otherNames ?? "",
    phoneNumber: m.phoneNumber,
    emailAddress: m.emailAddress ?? "",
    dateOfBirth: m.dateOfBirth.slice(0, 10),
    joinedDate: m.joinedDate?.slice(0, 10) ?? "",
    gender: m.gender,
    maritalStatus: m.maritalStatus,
    status: m.status,
    residentialAddress: m.residentialAddress,
    city: m.city,
    hometown: m.hometown,
    region: m.region,
    gpsAddress: m.gpsAddress ?? "",
    nextOfKin: m.nextOfKin,
    emergencyContactName: m.emergencyContactName,
    emergencyContactPhoneNumber: m.emergencyContactPhoneNumber,
  };
}

// The API reports a problem with "firstName"; the form calls that field "firstName" too, so the
// map is just every field mapped to itself.
export const fieldMap = Object.fromEntries(
  Object.keys(emptyMember).map((k) => [k, k]),
) as Record<keyof MemberValues, keyof MemberValues>;

// "Ama Serwaa Mensah" becomes first name "Ama", other names "Serwaa", last name "Mensah".
export function splitName(full: string) {
  const parts = full.trim().split(/\s+/);
  const firstName = parts[0] ?? "";
  const lastName = parts.length > 1 ? (parts[parts.length - 1] ?? "") : "";
  const otherNames = parts.slice(1, -1).join(" ");
  return { firstName, otherNames, lastName };
}

// The reverse of splitName, for showing the three parts as one name.
export const joinName = (first: string, other: string, last: string) =>
  [first, other, last].filter(Boolean).join(" ");
