import type { UserRole } from "@/features/account";
import { roleLabel } from "@/features/account";
import { formatNumber } from "@/lib/format";
import type { Part } from "@/ui";
import type { RoleCount } from "../api/team";

// The order of the cards, most access first. A role added later only needs a line here.
export const roleOrder: UserRole[] = ["Admin", "Clerk", "Accountant", "Member"];

export const roleOptions = roleOrder.map((r) => ({
  value: r,
  label: roleLabel(r),
}));

// The bar splits the team by role, biggest first. A role nobody holds is left out of the bar (the bar only
// draws parts with something in them) but keeps its card.
export function roleParts(roles: RoleCount[]): Part[] {
  return roles
    .map((r) => ({
      key: r.role,
      label: roleLabel(r.role),
      value: r.count,
      text: formatNumber(r.count),
    }))
    .sort((a, b) => b.value - a.value);
}

export const countOf = (roles: RoleCount[] | undefined, role: UserRole) =>
  roles?.find((r) => r.role === role)?.count;
