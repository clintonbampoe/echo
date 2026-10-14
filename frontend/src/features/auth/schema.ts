// What each auth form accepts, and the words shown when it does not. Server rules are the
// final say; these only catch the obvious mistakes before a round trip.
import { z } from "zod";

const required = (message: string) => z.string().trim().min(1, message);
const email = required("Enter your email address.").pipe(
  z.email("Enter a valid email address."),
);
const newPassword = z
  .string()
  .min(8, "Use at least 8 characters.")
  .max(128, "Use 128 characters or fewer.");

export const signInSchema = z.object({
  email,
  password: z.string().min(1, "Enter your password."),
});

export const emailOnlySchema = z.object({ email });

export const accountSchema = z.object({
  firstName: required("Enter your first name.").max(100),
  lastName: required("Enter your last name.").max(100),
  email,
  password: newPassword,
});

export const churchSchema = z.object({
  name: required("Enter your church name.").max(100),
  orgType: z.enum(["Church", "Mosque", "NonProfit", "Other"]),
  phoneNumber: required("Enter a phone number.").max(
    20,
    "Use 20 characters or fewer.",
  ),
  churchEmail: email,
  region: required("Choose a region."),
  city: required("Enter your city.").max(100),
  town: required("Enter your town.").max(100),
  gpsAddress: required("Enter your digital address.").max(255),
});

export const resetSchema = z
  .object({ newPassword, confirm: z.string() })
  .refine((v) => v.newPassword === v.confirm, {
    path: ["confirm"],
    message: "The passwords do not match.",
  });

export const codeSchema = z.object({
  code: required("Enter your invitation code."),
});

export const joinSchema = z.object({
  firstName: required("Enter your first name.").max(100),
  lastName: required("Enter your last name.").max(100),
  otherNames: z.string().trim().max(100),
  email,
  password: newPassword,
});

export type SignInValues = z.infer<typeof signInSchema>;
export type EmailOnlyValues = z.infer<typeof emailOnlySchema>;
export type AccountValues = z.infer<typeof accountSchema>;
export type ChurchValues = z.infer<typeof churchSchema>;
export type ResetValues = z.infer<typeof resetSchema>;
export type CodeValues = z.infer<typeof codeSchema>;
export type JoinValues = z.infer<typeof joinSchema>;
