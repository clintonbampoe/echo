// Who is signed in, and which church they belong to. No React in this file.
import { api } from "@/lib/api";

// What someone may do in the app. (Not the same as a role inside an organization.)
export type UserRole = "Admin" | "Accountant" | "Clerk" | "Member";

export type Me = {
  id: string;
  name: string;
  emailAddress: string;
  role: UserRole;
  // Null while the email address is not verified. Signing in does not need it.
  verifiedAt?: string | null;
  // Null for accounts older than the field.
  passwordChangedAt?: string | null;
};

export type Congregation = {
  id: string;
  name: string;
};

// Whether the signed-in user's email address has been verified. Cheap to ask again, for example when the
// person returns to the app after opening the link in their email.
export type VerificationStatus = {
  verified: boolean;
  verifiedAt: string | null;
  emailAddress: string;
};

export const getMe = (signal?: AbortSignal) =>
  api.get<Me>("/v1/users/me", { signal });

export const getCongregation = (signal?: AbortSignal) =>
  api.get<Congregation>("/v1/congregation", { signal });

export const getVerificationStatus = (signal?: AbortSignal) =>
  api.get<VerificationStatus>("/v1/auth/verifications/status", { signal });
