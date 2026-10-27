import { roleLabel } from "@/features/account";
import { formatDayMonth, plural } from "@/lib/format";
import type { Invitation } from "../api/invitations";

const DAY = 86_400_000;

// The row's title: the name the admin gave it, or what it lets people be.
export const titleOf = (i: Invitation) =>
  i.label || `${roleLabel(i.allowedRole)} invitation`;

// "Clerk · used 9 times · expires 31 Oct". Dates are read against `now` so the words stay true.
export function subLine(i: Invitation, now = Date.now()): string {
  const uses =
    i.useCount === 0 ? "not used yet" : `used ${plural(i.useCount, "time")}`;
  return `${roleLabel(i.allowedRole)} · ${uses} · ${expiry(i, now)}`;
}

function expiry(i: Invitation, now: number): string {
  if (i.status === "Revoked") return "switched off";
  if (i.status === "Expired") return `expired ${formatDayMonth(i.expiresAt)}`;
  const days = Math.ceil((Date.parse(i.expiresAt) - now) / DAY);
  if (days <= 0) return "expires today";
  if (days <= 7) return `expires in ${plural(days, "day")}`;
  return `expires ${formatDayMonth(i.expiresAt)}`;
}
