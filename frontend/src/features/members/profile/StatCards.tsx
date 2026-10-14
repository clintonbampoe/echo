import { usePersonStats } from "@/features/attendance";
import { useOrganizationsStat } from "@/features/organizations";
import { useTitheStat } from "@/features/tithes";
import { StatStrip } from "@/ui";

// The four numbers at the top of a profile. Each one is worked out by the feature it is about; attendance
// and tithes are for this year so far.
export function StatCards({ id, today }: { id: string; today: string }) {
  const [attendance, lastSeen] = usePersonStats(id, today);
  const tithes = useTitheStat(id, today);
  const organizations = useOrganizationsStat(id);
  return (
    <StatStrip
      label="Member numbers"
      stats={[attendance!, lastSeen!, tithes, organizations]}
    />
  );
}
