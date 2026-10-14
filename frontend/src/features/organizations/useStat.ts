import { failedStat, type Stat } from "@/ui";
import { usePersonMemberships } from "./hooks";
import { organizationsStat } from "./stat";

// The number a profile shows about a person's organizations: how many, and which.
export function useOrganizationsStat(personId: string): Stat {
  const list = usePersonMemberships(personId);
  const stat = organizationsStat(
    list.rows.map((m) => m.organizationName),
    list.loading,
  );
  return list.failed ? failedStat(stat) : stat;
}
