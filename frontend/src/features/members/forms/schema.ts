// What the member form accepts, and the words shown when it does not. The server has the final
// say; this catches the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { todayText } from "@/lib/dates";
import { required } from "@/lib/validation";

const phonePattern = /^\+?[\d\s()-]{7,20}$/;
const phone = (message: string) =>
  required(message).regex(phonePattern, "Enter a valid phone number");
const optionalPhone = z
  .string()
  .trim()
  .refine(
    (v) => v === "" || phonePattern.test(v),
    "Enter a valid phone number",
  );

// Dates are plain "2019-03-12" text, the same shape the API uses, so nothing is converted.
const birthDate = required("Enter a date of birth").refine(
  (v) => v <= todayText(),
  "Birth date cannot be in the future",
);

// Only a name, phone, gender and birth date are needed. The rest may be left empty and filled in later.
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
  gender: z.enum(["Male", "Female", "NonBinary"]),
  maritalStatus: z.enum(["", "Single", "Married", "Widowed"]),
  status: z.enum(["Active", "Inactive", "Archived", "Transferred"]),
  residentialAddress: z.string().trim().max(255),
  city: z.string().trim().max(100),
  hometown: z.string().trim().max(100),
  region: z.string(),
  gpsAddress: z.string().trim().max(255),
  nextOfKin: z.string().trim().max(100),
  emergencyContactName: z.string().trim().max(100),
  emergencyContactPhoneNumber: optionalPhone,
});

export type MemberValues = z.infer<typeof memberSchema>;
