// Who is signed in, and which church they belong to. No React in this file.
import { api } from "@/lib/api";

export type Role = "Admin" | "Accountant" | "Clerk" | "Member";

export type Me = {
  id: string;
  name: string;
  emailAddress: string;
  role: Role;
};

export type Congregation = {
  id: string;
  name: string;
};

export const getMe = (signal?: AbortSignal) =>
  api.get<Me>("/v1/users/me", { signal });

export const getCongregation = (signal?: AbortSignal) =>
  api.get<Congregation>("/v1/congregation", { signal });
