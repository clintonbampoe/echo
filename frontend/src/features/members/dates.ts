// How long someone has been a member, from plain "2019-03-12" dates. Pure, so the tests can pass "today".
import { dayParts } from "@/lib/dates";

// "Member for 7 years", "Member for 5 months", or "New member" in the first month.
// Returns an empty string when we do not know the joined date.
export function memberFor(
  joined: string | null | undefined,
  today: string,
): string {
  if (!joined) return "";
  const [jy, jm, jd] = dayParts(joined);
  const [ty, tm, td] = dayParts(today);
  const months = (ty - jy) * 12 + (tm - jm) - (td < jd ? 1 : 0);
  if (months < 1) return "New member";
  if (months < 12)
    return `Member for ${months} ${months === 1 ? "month" : "months"}`;
  const years = Math.floor(months / 12);
  return `Member for ${years} ${years === 1 ? "year" : "years"}`;
}
