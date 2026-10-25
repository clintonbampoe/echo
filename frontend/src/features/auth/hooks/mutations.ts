import { useMutation, useQueryClient } from "@tanstack/react-query";
import { clearTokens, getTokens, saveTokens } from "@/lib/tokens";
import {
  forgotPassword,
  login,
  lookupInvitation,
  registerCongregation,
  registerMember,
  resetPassword,
  revokeSession,
  sendVerificationLink,
} from "../api/auth";
import { authKeys } from "./keys";

export function useSignIn() {
  return useMutation({
    mutationFn: (v: { email: string; password: string }) =>
      login(v.email, v.password),
    onSuccess: saveTokens,
  });
}

// Ends this device's session. Local sign-out always happens, even if the server call fails.
export function useSignOut() {
  return useMutation({
    mutationFn: async () => {
      const tokens = getTokens();
      try {
        if (tokens) await revokeSession(tokens.refreshToken);
      } catch {
        // Revoking is best effort: the tokens are cleared below either way.
      }
    },
    onSettled: clearTokens,
  });
}

export const useRegisterCongregation = () =>
  useMutation({ mutationFn: registerCongregation });

export const useRegisterMember = () =>
  useMutation({ mutationFn: registerMember });

export const useSendVerification = () =>
  useMutation({ mutationFn: sendVerificationLink });

export const useForgotPassword = () =>
  useMutation({ mutationFn: forgotPassword });

export const useResetPassword = () =>
  useMutation({
    mutationFn: (v: { token: string; newPassword: string }) =>
      resetPassword(v.token, v.newPassword),
  });

// Checks a typed-in code before leaving the page. The answer is cached, so the join page that
// follows shows the church straight away instead of asking again.
export function useCheckInvitation() {
  const client = useQueryClient();
  return (code: string) =>
    client.fetchQuery({
      queryKey: authKeys.invitation(code),
      queryFn: ({ signal }) => lookupInvitation(code, signal),
      staleTime: 60_000,
    });
}
