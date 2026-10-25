import { api } from "@/lib/api";

export const get = <T>(
  path: string,
  from: string,
  to: string,
  signal?: AbortSignal,
) => api.get<T>(path, { query: { from, to }, signal });
