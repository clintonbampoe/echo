import { useQuery } from "@tanstack/react-query";
import { getCongregation, getMe } from "../api/account";
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
