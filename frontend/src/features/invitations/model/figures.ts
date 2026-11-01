import { formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { InvitationSummary } from "../api/invitations";

const count = (value: number | undefined) =>
  value === undefined ? "–" : formatNumber(value);

// The three numbers above the list. The server counts every invitation, so they stay right past the
// first page; a dash shows while they load.
export const invitationStats = (s: InvitationSummary | undefined): Stat[] => [
  { label: "Active", value: count(s?.active), note: "" },
  { label: "People joined", value: count(s?.peopleJoined), note: "" },
  {
    label: "Expiring soon",
    value: count(s?.expiringSoon),
    note: "within 7 days",
  },
];
