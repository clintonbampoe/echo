import type { UserRole } from "@/features/account";
import type { TeamStatus } from "../api/team";

// Every cache key the team uses, in one place.
export const teamKeys = {
  all: ["team"] as const,
  summary: ["team", "summary"] as const,
  role: (role: UserRole, status: TeamStatus | undefined) =>
    ["team", "role", role, status ?? "all"] as const,
};
