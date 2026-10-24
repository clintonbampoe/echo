import { usePersonStats } from "@/features/attendance";
import { useLastTitheStat, useTitheStat } from "@/features/tithes";
import { StatStrip } from "@/ui";

// The four numbers at the top of a profile. Each one is worked out by the feature it is about; attendance
// and tithes are for this year so far.
export function ProfileFigures({ id, today }: { id: string; today: string }) {
  const [attendance, lastSeen] = usePersonStats(id, today);
  const tithes = useTitheStat(id, today);
  const lastTithe = useLastTitheStat(id);
  return (
    <StatStrip
      label="Member numbers"
      stats={[attendance!, tithes, lastSeen!, lastTithe]}
    />
  );
}
