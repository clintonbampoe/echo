import { formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { Invitation } from "../api/invitations";

const WEEK = 7 * 86_400_000;

// The three numbers above the list, counted from the invitations loaded so far.
export function invitationStats(rows: Invitation[], now = Date.now()): Stat[] {
  const active = rows.filter((i) => i.status === "Active");
  const soon = active.filter((i) => Date.parse(i.expiresAt) - now <= WEEK);
  const joined = rows.reduce((sum, i) => sum + i.useCount, 0);
  return [
    { label: "Active", value: formatNumber(active.length), note: "" },
    { label: "People joined", value: formatNumber(joined), note: "" },
    {
      label: "Expiring soon",
      value: formatNumber(soon.length),
      note: "within 7 days",
    },
  ];
}
