// What the membership sheet changes about one person's place in a group: their role and the day they joined.
// No React in this file.
import { addMemberSchema } from "./addMember";
import type { OrganizationMember, OrganizationMemberInput } from "./api";
import { roles } from "./view";

export const membershipSchema = addMemberSchema.pick({
  role: true,
  joinedAt: true,
});

export type MembershipValues = { role: string; joinedAt: string };

export const fromMembership = (
  m: Pick<OrganizationMember, "role" | "joinedAt">,
): MembershipValues => ({
  role: m.role,
  joinedAt: m.joinedAt.slice(0, 10),
});

export const toMembershipInput = (
  v: MembershipValues,
): Pick<OrganizationMemberInput, "role" | "joinedAt"> => ({
  role: roles.find((r) => r === v.role) ?? "Member",
  joinedAt: v.joinedAt,
});
