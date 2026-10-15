import { thisYear } from "@/lib/dates";
import { failedStat, type Stat } from "@/ui";
import { useTitheMonths } from "./hooks";
import { tithesStat } from "./stat";

// The number a profile shows about a member's tithes: what they gave this year so far.
export function useTitheStat(memberId: string, today: string): Stat {
  const year = thisYear(today);
  const months = useTitheMonths({ memberId }, year.from, year.to);
  const stat = tithesStat(months.data, today);
  return months.isError ? failedStat(stat) : stat;
}
