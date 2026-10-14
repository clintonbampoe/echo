// What the member form accepts, and the words shown when it does not. The server has the final
// say; this catches the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { todayText } from "@/lib/dates";
import { required } from "@/lib/validation";
import type { Member, MemberInput } from "./api";

const phone = (message: string) =>
  required(message).regex(/^\+?[\d\s()-]{7,20}$/, "Enter a valid phone number");

// Dates are plain "2019-03-12" text, the same shape the API uses, so nothing is converted.
const birthDate = required("Enter a date of birth").refine(
  (v) => v <= todayText(),
  "Birth date cannot be in the future",
);

export const memberSchema = z.object({
  firstName: required("Enter a first name").max(100),
  lastName: required("Enter a last name").max(100),
  otherNames: z.string().trim().max(100),
  phoneNumber: phone("Enter a phone number"),
  emailAddress: z.union([
    z.literal(""),
    z.email("Enter a valid email address"),
  ]),
  dateOfBirth: birthDate,
  joinedDate: z.string(),
  gender: z.enum(["Male", "Female", "Other"]),
  maritalStatus: z.enum(["Single", "Married", "Widowed"]),
  status: z.enum(["Active", "Inactive", "Archived", "Transferred"]),
  residentialAddress: required("Enter a residential address").max(255),
  city: required("Enter a city").max(100),
  hometown: required("Enter a hometown").max(100),
  region: required("Choose a region"),
  gpsAddress: z.string().trim().max(255),
  nextOfKin: required("Enter a next of kin").max(100),
  emergencyContactName: required("Enter a contact name").max(100),
  emergencyContactPhoneNumber: phone("Enter a contact phone"),
});

export type MemberValues = z.infer<typeof memberSchema>;

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
