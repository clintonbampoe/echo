import { roleLabel } from "@/features/account";
import { formatDayMonth, plural } from "@/lib/format";
import type { Invitation } from "../api/invitations";

const DAY = 86_400_000;

// The row's title: the name the admin gave it, or what it lets people be.
export const titleOf = (i: Invitation) =>
  i.label || `${roleLabel(i.allowedRole)} invitation`;

// "Clerk · used 9 times · expires 31 Oct". Dates are read against `now` so the words stay true.
export function subLine(i: Invitation, now = Date.now()): string {
  return `${roleLabel(i.allowedRole)} · ${uses(i)} · ${expiry(i, now)}`;
}

function uses(i: Invitation): string {
  if (i.maxUses) return `${i.useCount} of ${i.maxUses} used`;
  return i.useCount === 0
    ? "not used yet"
    : `used ${plural(i.useCount, "time")}`;
}

function expiry(i: Invitation, now: number): string {
  if (i.status === "Revoked") return "switched off";
  if (i.status === "Exhausted") return "all places taken";
  if (!i.expiresAt) return "never expires";
  if (i.status === "Expired") return `expired ${formatDayMonth(i.expiresAt)}`;
  const days = Math.ceil((Date.parse(i.expiresAt) - now) / DAY);
  if (days <= 0) return "expires today";
  if (days <= 7) return `expires in ${plural(days, "day")}`;
  return `expires ${formatDayMonth(i.expiresAt)}`;
}

export const statusLabel: Record<Invitation["status"], string> = {
  Active: "Active",
  Expired: "Expired",
  Revoked: "Switched off",
  Exhausted: "All places taken",
};

// The address people open to join. The join screen reads the code from the path.
export const joinLink = (code: string, origin = window.location.origin) =>
  `${origin}/join/${encodeURIComponent(code)}`;
