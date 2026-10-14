// The only file in account that knows about TanStack Query.
import { useQuery } from "@tanstack/react-query";
import * as api from "./api";

export const accountKeys = {
  me: ["account", "me"] as const,
  congregation: ["account", "congregation"] as const,
};

// These change rarely, so a few minutes without a refetch is fine.
const FIVE_MINUTES = 5 * 60_000;

export const useMe = () =>
  useQuery({
    queryKey: accountKeys.me,
    queryFn: ({ signal }) => api.getMe(signal),
    staleTime: FIVE_MINUTES,
  });

export const useCongregation = () =>
  useQuery({
    queryKey: accountKeys.congregation,
    queryFn: ({ signal }) => api.getCongregation(signal),
    staleTime: FIVE_MINUTES,
  });
