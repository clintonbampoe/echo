// What the visitor form accepts, and the words shown when it does not. The server has the final
// say; this catches the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { required } from "@/lib/validation";
import type { Visitor, VisitorInput } from "../api/visitors";

// A name and a gender are needed (visitors give their gender, not their age). Everything else may be left empty, but when it is filled it must make sense.
const optionalPhone = z
  .string()
  .trim()
  .refine(
    (v) => v === "" || /^\+?[\d\s()-]{7,20}$/.test(v),
    "Enter a valid phone number",
  );

export const NOTES_MAX = 2000;

export const visitorSchema = z.object({
  firstName: required("Enter a first name").max(100),
  lastName: required("Enter a last name").max(100),
  phoneNumber: optionalPhone,
  emailAddress: z.union([
    z.literal(""),
    z.email("Enter a valid email address"),
  ]),
  gender: z.enum(["Male", "Female", "NonBinary"], "Choose a gender"),
  notes: z.string().max(NOTES_MAX),
});

export type VisitorValues = z.infer<typeof visitorSchema>;

export const emptyVisitor: VisitorValues = {
  firstName: "",
  lastName: "",
  phoneNumber: "",
  emailAddress: "",
  gender: "" as VisitorValues["gender"],
  notes: "",
};

// The form holds text for every field; the API wants optional ones left out when empty.
export function toInput(v: VisitorValues): VisitorInput {
  return {
    firstName: v.firstName,
    lastName: v.lastName,
    phoneNumber: v.phoneNumber || undefined,
    emailAddress: v.emailAddress || undefined,
    gender: v.gender,
    notes: v.notes.trim() || undefined,
  };
}

// Fills the edit form from a visitor the server sent. Missing optional values become empty text.
export function fromVisitor(v: Visitor): VisitorValues {
  return {
    firstName: v.firstName,
    lastName: v.lastName,
    phoneNumber: v.phoneNumber ?? "",
    emailAddress: v.emailAddress ?? "",
    gender: v.gender ?? ("" as VisitorValues["gender"]),
    notes: v.notes ?? "",
  };
}

// The API reports a problem with "firstName"; the form calls that field "firstName" too, so the
// map is just every field mapped to itself.
export const fieldMap = Object.fromEntries(
  Object.keys(emptyVisitor).map((k) => [k, k]),
) as Record<keyof VisitorValues, keyof VisitorValues>;
