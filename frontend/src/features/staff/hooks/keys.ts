import type { UserRole } from "@/features/account";
import type { StaffQuery } from "../api/staff";

// Every cache key staff uses, in one place.
export const staffKeys = {
  all: ["staff"] as const,
  summary: ["staff", "summary"] as const,
  role: (role: UserRole, query: StaffQuery) =>
    [
      "staff",
      "role",
      role,
      query.status ?? "all",
      query.q ?? "",
      query.size ?? "page",
    ] as const,
};
