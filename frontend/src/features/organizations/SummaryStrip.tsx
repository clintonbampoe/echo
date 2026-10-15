import { formatNumber } from "@/lib/format";
import { StatStrip, type Stat } from "@/ui";
import type { OrganizationSummary } from "./api";

type Props = { summary?: OrganizationSummary };

// The four numbers above the list. They follow the time range, not the search.
export function SummaryStrip({ summary: s }: Props) {
  const n = (value: number | undefined) =>
    s && value !== undefined ? formatNumber(value) : "–";
  const largest = s?.largestOrganization;
  const stats: Stat[] = [
    { label: "Organizations", value: n(s?.totalOrganizations), note: "Active" },
    {
      label: "Memberships",
      value: n(s?.totalMembers),
      note: "Across all groups",
    },
    {
      label: "Average size",
      value: n(s && Math.round(s.averageMembersPerOrganization)),
      note: "Members per group",
    },
    {
      label: "Largest",
      value: largest ?? "–",
      note: largest ? "Most members" : "No groups yet",
    },
  ];
  return <StatStrip label="Organization summary" stats={stats} busy={!s} />;
}
