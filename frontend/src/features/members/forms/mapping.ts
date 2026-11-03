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
  maritalStatus: "",
  status: "Active",
  residentialAddress: "",
  city: "",
  hometown: "",
  region: "",
  gpsAddress: "",
  nextOfKin: "",
  emergencyContactName: "",
  emergencyContactPhoneNumber: "",
};

// An empty form field is left out of what is sent.
const orUndefined = <T extends string>(text: T | "") => text || undefined;
// A missing value shows as an empty field.
const orEmpty = (value?: string | null) => value ?? "";

// The form holds text for every field; the API wants optional ones left out when empty.
export function toInput(v: MemberValues): MemberInput {
  return {
    ...v,
    otherNames: orUndefined(v.otherNames),
    emailAddress: orUndefined(v.emailAddress),
    joinedDate: orUndefined(v.joinedDate),
    gpsAddress: orUndefined(v.gpsAddress),
    residentialAddress: orUndefined(v.residentialAddress),
    city: orUndefined(v.city),
    hometown: orUndefined(v.hometown),
    region: orUndefined(v.region),
    maritalStatus: orUndefined(v.maritalStatus),
    nextOfKin: orUndefined(v.nextOfKin),
    emergencyContactName: orUndefined(v.emergencyContactName),
    emergencyContactPhoneNumber: orUndefined(v.emergencyContactPhoneNumber),
  };
}

// Fills the edit form from a member the server sent. Missing optional values become empty text.
export function fromMember(m: Member): MemberValues {
  return {
    firstName: m.firstName,
    lastName: m.lastName,
    otherNames: orEmpty(m.otherNames),
    phoneNumber: m.phoneNumber,
    emailAddress: orEmpty(m.emailAddress),
    dateOfBirth: m.dateOfBirth.slice(0, 10),
    joinedDate: orEmpty(m.joinedDate).slice(0, 10),
    gender: m.gender,
    maritalStatus: m.maritalStatus ?? "",
    status: m.status,
    residentialAddress: orEmpty(m.residentialAddress),
    city: orEmpty(m.city),
    hometown: orEmpty(m.hometown),
    region: orEmpty(m.region),
    gpsAddress: orEmpty(m.gpsAddress),
    nextOfKin: orEmpty(m.nextOfKin),
    emergencyContactName: orEmpty(m.emergencyContactName),
    emergencyContactPhoneNumber: orEmpty(m.emergencyContactPhoneNumber),
  };
}

// The API reports a problem with "firstName"; the form calls that field "firstName" too, so the
// map is just every field mapped to itself.
export const fieldMap = Object.fromEntries(
  Object.keys(emptyMember).map((k) => [k, k]),
) as Record<keyof MemberValues, keyof MemberValues>;
