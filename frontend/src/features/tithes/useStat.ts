import { thisYear } from "@/lib/dates";
import { failedStat, type Stat } from "@/ui";
import { useMemberTithes, useTitheMonths } from "./hooks";
import { lastTitheStat, tithesStat } from "./stat";

// The number a profile shows about a member's tithes: what they gave this year so far.
export function useTitheStat(memberId: string, today: string): Stat {
  const year = thisYear(today);
  const months = useTitheMonths({ memberId }, year.from, year.to);
  const stat = tithesStat(months.data, today);
  return months.isError ? failedStat(stat) : stat;
}

// The date of a member's newest tithe, as a card.
export function useLastTitheStat(memberId: string): Stat {
  const list = useMemberTithes(memberId);
  const stat = lastTitheStat(list.rows[0]?.collectionDate, list.loading);
  return list.failed ? failedStat(stat) : stat;
}
