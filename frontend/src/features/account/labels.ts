import type { Role } from "./api";

const roleLabels: Record<Role, string> = {
  Admin: "Administrator",
  Accountant: "Accountant",
  Clerk: "Clerk",
  Member: "Member",
};

export const roleLabel = (role: Role) => roleLabels[role];

// "Ama Mensah" -> "Ama". Used to greet people.
export const firstName = (name: string) => name.trim().split(/\s+/)[0] ?? "";
