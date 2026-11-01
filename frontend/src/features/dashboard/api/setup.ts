import { api } from "@/lib/api";

// How many invitations are open or have been used. The invitations page counts them on the server.
export const hasInvitations = (signal?: AbortSignal) =>
  api
    .get<{ active: number; peopleJoined: number }>(
      "/v1/auth/invitations/summary",
      { signal },
    )
    .then((r) => r.active + r.peopleJoined > 0);
