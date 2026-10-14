// The only file in auth that knows about TanStack Query.
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { isApiError } from "@/lib/api";
import { clearTokens, getTokens, saveTokens } from "@/lib/tokens";
import * as api from "./api";

export const authKeys = {
  invitation: (code: string) => ["auth", "invitation", code] as const,
  verify: (token: string) => ["auth", "verify", token] as const,
};

export function useSignIn() {
  return useMutation({
    mutationFn: (v: { email: string; password: string }) =>
      api.login(v.email, v.password),
    onSuccess: saveTokens,
  });
}

// Ends this device's session. Local sign-out always happens, even if the server call fails.
export function useSignOut() {
  return useMutation({
    mutationFn: async () => {
      const tokens = getTokens();
      try {
        if (tokens) await api.revokeSession(tokens.refreshToken);
      } catch {
        // Revoking is best effort: the tokens are cleared below either way.
      }
    },
    onSettled: clearTokens,
  });
}

export const useRegisterCongregation = () =>
  useMutation({ mutationFn: api.registerCongregation });
export const useRegisterMember = () =>
  useMutation({ mutationFn: api.registerMember });
export const useSendVerification = () =>
  useMutation({ mutationFn: api.sendVerificationLink });
export const useForgotPassword = () =>
  useMutation({ mutationFn: api.forgotPassword });
export const useResetPassword = () =>
  useMutation({
    mutationFn: (v: { token: string; newPassword: string }) =>
      api.resetPassword(v.token, v.newPassword),
  });

// Opening the link in the email is what verifies the account, so this runs as a query. A
// query, unlike an effect, runs once even when React mounts the page twice in development; a
// second request would find the single-use token already spent and wrongly report it expired.
export function useVerifyEmailLink(token: string) {
  return useQuery({
    queryKey: authKeys.verify(token),
    queryFn: () => api.verifyEmail(token).then(() => true),
    enabled: token !== "",
    retry: false,
    staleTime: Infinity,
    gcTime: Infinity,
  });
}

export function useInvitation(code: string) {
  return useQuery({
    queryKey: authKeys.invitation(code),
    queryFn: ({ signal }) => api.lookupInvitation(code, signal),
    enabled: code !== "",
    // A wrong or expired code is a final answer, not something to retry.
    retry: (count, error) =>
      !(isApiError(error) && error.status === 404) && count < 2,
  });
}

// Checks a typed-in code before leaving the page. The answer is cached, so the join page that
// follows shows the church straight away instead of asking again.
export function useCheckInvitation() {
  const client = useQueryClient();
  return (code: string) =>
    client.fetchQuery({
      queryKey: authKeys.invitation(code),
      queryFn: ({ signal }) => api.lookupInvitation(code, signal),
      staleTime: 60_000,
    });
}
