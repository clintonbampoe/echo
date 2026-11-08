import { formatDate } from "@/lib/format";
import type { ChipTone } from "@/ui";
import type { TeamUser } from "../api/team";

export const isDisabled = (u: TeamUser) => u.disabledAt !== null;

// What the status chip says, if anything. An ordinary active, verified account has nothing to flag.
export function flag(
  u: TeamUser,
  isMe: boolean,
): { label: string; tone: ChipTone } | null {
  if (isMe) return { label: "You", tone: "outline" };
  if (isDisabled(u)) return { label: "Disabled", tone: "neutral" };
  if (u.verifiedAt === null) return { label: "Unverified", tone: "outline" };
  return null;
}

export const statusText = (u: TeamUser) =>
  isDisabled(u) ? "Disabled" : "Active";

export const dayOf = (iso: string) => formatDate(iso.slice(0, 10));
