import { useQuery } from "@tanstack/react-query";
import { isApiError } from "@/lib/api";
import { lookupInvitation, verifyEmail } from "../api/auth";
import { authKeys } from "./keys";

// Opening the link in the email is what verifies the account, so this runs as a query. A
// query, unlike an effect, runs once even when React mounts the page twice in development; a
// second request would find the single-use token already spent and wrongly report it expired.
export function useVerifyEmailLink(token: string) {
  return useQuery({
    queryKey: authKeys.verify(token),
    queryFn: () => verifyEmail(token).then(() => true),
    enabled: token !== "",
    retry: false,
    staleTime: Infinity,
    gcTime: Infinity,
  });
}

export function useInvitation(code: string) {
  return useQuery({
    queryKey: authKeys.invitation(code),
    queryFn: ({ signal }) => lookupInvitation(code, signal),
    enabled: code !== "",
    // A wrong or expired code is a final answer, not something to retry.
    retry: (count, error) =>
      !(isApiError(error) && error.status === 404) && count < 2,
  });
}
