// What the "Make member" form starts with. Pure, so the tests need no screen. No React in this file.
import { emptyMember, type MemberValues } from "@/features/members";
import type { Visitor } from "./api";

// What a visitor record already knows is filled in; the rest of the member form is for staff to complete.
export function toMemberValues(v: Visitor): MemberValues {
  return {
    ...emptyMember,
    firstName: v.firstName,
    lastName: v.lastName,
    phoneNumber: v.phoneNumber ?? "",
    emailAddress: v.emailAddress ?? "",
    dateOfBirth: v.dateOfBirth?.slice(0, 10) ?? "",
  };
}
