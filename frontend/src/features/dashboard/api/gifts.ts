import { get } from "./client";

// What was given to projects in the range.
export const getGifts = (from: string, to: string, signal?: AbortSignal) =>
  get<{ totalContributed: number }>(
    "/v1/project-contributions/summary",
    from,
    to,
    signal,
  ).then((r) => r.totalContributed);
