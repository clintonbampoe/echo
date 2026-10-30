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
  emailVerifiedAt?: string | null;
  // Null for accounts older than the field.
  passwordChangedAt?: string | null;
};

export type Congregation = {
  id: string;
  name: string;
};

export const getMe = (signal?: AbortSignal) =>
  api.get<Me>("/v1/users/me", { signal });

export const getCongregation = (signal?: AbortSignal) =>
  api.get<Congregation>("/v1/congregation", { signal });
