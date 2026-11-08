import { StatStrip } from "@/ui";
import { formatNumber } from "@/lib/format";
import type { UserRole } from "@/features/account";
import type { StaffSummary } from "../api/staff";
import { shareOf } from "../model/figures";
import { countOf } from "../model/roles";

// The role at a glance: how many hold it, their share of staff, and how many are disabled. Head counts come from the
// summary (the whole congregation), so they stay right whatever the list below is searching for.
export function RoleStrip({
  role,
  summary,
}: {
  role: UserRole;
  summary: StaffSummary | undefined;
}) {
  const entry = summary?.roles.find((r) => r.role === role);
  const people = countOf(summary?.roles, role);
  return (
    <StatStrip
      label="This role at a glance"
      busy={!summary}
      stats={[
        { label: "People", value: formatNumber(people ?? 0) },
        {
          label: "Share of staff",
          value: shareOf(people, summary?.total ?? 0),
        },
        { label: "Disabled", value: formatNumber(entry?.disabled ?? 0) },
      ]}
    />
  );
}
