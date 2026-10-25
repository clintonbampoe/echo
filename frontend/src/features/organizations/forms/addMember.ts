// What the "Add member" form accepts, and the words shown when it does not. No React in this file.
import { z } from "zod";
import { required } from "@/lib/validation";
import type { OrganizationMemberInput } from "../api/memberships";
import { roles } from "../model/display";

export const addMemberSchema = z.object({
  memberId: required("Choose a member"),
  role: z.enum(roles as [string, ...string[]]),
  joinedAt: required("Enter the day they joined"),
});

export type AddMemberValues = z.infer<typeof addMemberSchema>;

export const emptyAddMember = (today: string): AddMemberValues => ({
  memberId: "",
  role: "Member",
  joinedAt: today,
});

export const addMemberFieldMap: Record<string, keyof AddMemberValues> = {
  memberId: "memberId",
  role: "role",
  joinedAt: "joinedAt",
};

export function toMemberInput(
  organizationId: string,
  v: AddMemberValues,
): OrganizationMemberInput {
  return {
    memberId: v.memberId,
    organizationId,
    role: roles.find((r) => r === v.role) ?? "Member",
    joinedAt: v.joinedAt,
  };
}
