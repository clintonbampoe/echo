// What the member form accepts, and the words shown when it does not. The server has the final
// say; this catches the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { todayText } from "@/lib/dates";
import { required } from "@/lib/validation";

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
