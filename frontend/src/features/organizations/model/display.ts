// Words and numbers an organization is shown with. Pure, so the tests need no screen.
import { formatNumber } from "@/lib/format";
import type {
  OrganizationMemberFilters,
  OrganizationRole,
} from "../api/memberships";

// "61 members", "1 member".
export const memberCountLabel = (count: number): string =>
  `${formatNumber(count)} ${count === 1 ? "member" : "members"}`;

// Every role, the one with the most to do first. The role filter and the pickers list them in this order.
export const roles: readonly OrganizationRole[] = [
  "Leader",
  "Secretary",
  "Member",
];

// The members list's role filter holds text ('' for any role) and so does its search; the API wants a role or a
// name, or nothing.
export const toMemberFilters = (
  role: string,
  name = "",
): OrganizationMemberFilters => ({
  role: roles.find((r) => r === role),
  name: name.trim() || undefined,
});
