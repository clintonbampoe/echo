import type { UserRole } from "./api";

const roleLabels: Record<UserRole, string> = {
  Admin: "Administrator",
  Accountant: "Accountant",
  Clerk: "Clerk",
  Member: "Member",
};

export const roleLabel = (role: UserRole) => roleLabels[role];
