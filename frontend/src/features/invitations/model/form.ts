import { z } from "zod";
import type { InvitationInput, InvitationUpdate } from "../api/invitations";

// The server allows 1 to 10000. Empty means no limit.
export const maxUsesField = z
  .string()
  .trim()
  .refine((v) => v === "" || (/^\d+$/.test(v) && +v >= 1 && +v <= 10_000), {
    message: "Enter a number from 1 to 10,000, or leave it empty",
  });

export const invitationSchema = z.object({
  label: z.string().trim().max(100, "Keep the name under 100 letters"),
  allowedRole: z.enum(["Admin", "Accountant", "Clerk", "Member"]),
  expiryDays: z.enum(["1", "7", "14", "30", "90", "never"]),
  maxUses: maxUsesField,
});

export type InvitationValues = z.infer<typeof invitationSchema>;

export const emptyInvitation: InvitationValues = {
  label: "",
  allowedRole: "Clerk",
  expiryDays: "7",
  maxUses: "",
};

// "never" is its own flag on the API; any other choice is a number of days.
export const expiryInput = (
  v: string,
): Pick<InvitationUpdate, "expiryDays" | "neverExpires"> =>
  v === "never" ? { neverExpires: true } : { expiryDays: Number(v) };

export const toInput = (v: InvitationValues): InvitationInput => ({
  allowedRole: v.allowedRole,
  ...expiryInput(v.expiryDays),
  ...(v.maxUses ? { maxUses: Number(v.maxUses) } : {}),
  ...(v.label ? { label: v.label } : {}),
});

// The API's field names, so a message about one lands under the matching input.
export const fieldMap = {
  label: "label",
  allowedRole: "allowedRole",
  expiryDays: "expiryDays",
  maxUses: "maxUses",
} as const;
