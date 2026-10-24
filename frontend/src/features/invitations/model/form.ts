import { z } from "zod";
import type { InvitationInput } from "../api/invitations";

export const invitationSchema = z.object({
  label: z.string().trim().max(100, "Keep the name under 100 letters"),
  allowedRole: z.enum(["Admin", "Accountant", "Clerk", "Member"]),
  expiryDays: z.enum(["1", "7", "14", "30"]),
});

export type InvitationValues = z.infer<typeof invitationSchema>;

export const emptyInvitation: InvitationValues = {
  label: "",
  allowedRole: "Clerk",
  expiryDays: "7",
};

export const toInput = (v: InvitationValues): InvitationInput => ({
  allowedRole: v.allowedRole,
  expiryDays: Number(v.expiryDays),
  ...(v.label ? { label: v.label } : {}),
});

// The API's field names, so a message about one lands under the matching input.
export const fieldMap = {
  label: "label",
  allowedRole: "allowedRole",
  expiryDays: "expiryDays",
} as const;
