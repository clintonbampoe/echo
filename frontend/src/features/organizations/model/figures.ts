// The numbers about an organization, and the one about a person's organizations for a profile. Pure, so
// the tests can check them without a screen.
import { daysAfter } from "@/lib/dates";
import { formatAgo, formatDate, formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { Organization } from "../api/organizations";

export function organizationsStat(names: string[], loading: boolean): Stat {
  const more = names.length > 3 ? ` +${names.length - 3}` : "";
  return {
    label: "Organizations",
    value: loading ? "–" : String(names.length),
    note: names.slice(0, 3).join(", ") + more,
  };
}

export const membersStat = (o: Pick<Organization, "memberCount">): Stat => ({
  label: "Members",
  value: formatNumber(o.memberCount),
  note: "In this group",
});

// Who leads the group: the first leader's name, and who shares the lead with them.
export function ledByStat(leaders: string[], loading: boolean): Stat {
  const [first] = leaders;
  if (loading) return { label: "Led by", value: "–", note: "Loading" };
  if (!first) return { label: "Led by", value: "–", note: "No leader yet" };
  const others = leaders.length - 1;
  const note =
    others === 0
      ? "Leads this group"
      : others === 1
        ? `and ${leaders[1] ?? ""}`
        : `and ${others} others`;
  return { label: "Led by", value: first, note };
}

const SOON_DAYS = 60;

// How many events the group has coming in the next 60 days. `starts` is every upcoming event's day, soonest
// first, so when more are still to load and all of these fall in the window, the real count is higher.
export function upcomingStat(
  starts: string[],
  hasMore: boolean,
  today: string,
  loading: boolean,
): Stat {
  const note = `Next ${SOON_DAYS} days`;
  if (loading) return { label: "Upcoming events", value: "–", note };
  const last = daysAfter(today, SOON_DAYS);
  const soon = starts.filter((day) => day.slice(0, 10) <= last).length;
  return {
    label: "Upcoming events",
    value: hasMore && soon === starts.length ? `${soon}+` : String(soon),
    note,
  };
}

export const createdStat = (
  o: Pick<Organization, "createdAt">,
  today: string,
): Stat => ({
  label: "Created",
  value: formatDate(o.createdAt),
  note: formatAgo(o.createdAt.slice(0, 10), today),
});
