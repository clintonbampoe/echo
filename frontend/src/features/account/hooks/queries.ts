import { useEffect } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getCongregation, getMe, getVerificationStatus } from "../api/account";
import { accountKeys } from "./keys";

// These change rarely, so a few minutes without a refetch is fine.
const FIVE_MINUTES = 5 * 60_000;

export const useMe = () =>
  useQuery({
    queryKey: accountKeys.me,
    queryFn: ({ signal }) => getMe(signal),
    staleTime: FIVE_MINUTES,
  });

export const useCongregation = () =>
  useQuery({
    queryKey: accountKeys.congregation,
    queryFn: ({ signal }) => getCongregation(signal),
    staleTime: FIVE_MINUTES,
  });

// Asks whether the email address is verified, each time the person comes back to the tab. Switch it on only
// once a link has been sent. The moment the answer is yes, the profile is read again so the whole app knows.
export function useVerificationStatus(enabled: boolean) {
  const queryClient = useQueryClient();
  const status = useQuery({
    queryKey: accountKeys.verification,
    queryFn: ({ signal }) => getVerificationStatus(signal),
    enabled,
    refetchOnWindowFocus: "always",
  });
  const verified = status.data?.verified === true;
  useEffect(() => {
    if (verified)
      void queryClient.invalidateQueries({ queryKey: accountKeys.me });
  }, [verified, queryClient]);
  return status;
}
