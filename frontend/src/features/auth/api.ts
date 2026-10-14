// Every call here is anonymous: nothing in auth needs a token to start. No React in this file.
import type { CongregationType } from "@/features/account";
import { api } from "@/lib/api";
import type { TokenPair } from "@/lib/tokens";

const anon = { auth: false } as const;

export type RegisterCongregationInput = {
  congregationDto: {
    name: string;
    orgType: CongregationType;
    phoneNumber: string;
    emailAddress: string;
    region: string;
    city: string;
    town: string;
    gpsAddress: string;
  };
  userDto: {
    firstName: string;
    lastName: string;
    emailAddress: string;
    password: string;
  };
};

export type RegisterMemberInput = {
  token: string;
  userInfo: {
    firstName: string;
    lastName: string;
    otherNames?: string;
    emailAddress: string;
    password: string;
  };
};

export type InvitationInfo = {
  congregationName: string;
  city: string;
  region: string;
  allowedRole: string;
  expiresAt: string;
};

export const login = (email: string, password: string) =>
  api.post<TokenPair>("/v1/auth/sessions/login", { email, password }, anon);

export const revokeSession = (refreshToken: string) =>
  api.post("/v1/auth/sessions/revoke", { refreshToken }, anon);

export const registerCongregation = (input: RegisterCongregationInput) =>
  api.post("/v1/auth/registrations/congregation", input, anon);

export const registerMember = (input: RegisterMemberInput) =>
  api.post("/v1/auth/registrations/member", input, anon);

export const sendVerificationLink = (email: string) =>
  api.post("/v1/auth/verifications/account", { email }, anon);

// The token travels in the query string, not the body.
export const verifyEmail = (token: string) =>
  api.post("/v1/auth/verifications/verify-email", undefined, {
    ...anon,
    query: { token },
  });

export const forgotPassword = (email: string) =>
  api.post("/v1/auth/passwords/forgot", { email }, anon);

export const resetPassword = (token: string, newPassword: string) =>
  api.post("/v1/auth/passwords/reset", { token, newPassword }, anon);

export const lookupInvitation = (code: string, signal?: AbortSignal) =>
  api.get<InvitationInfo>(`/v1/auth/invitations/${encodeURIComponent(code)}`, {
    ...anon,
    signal,
  });
