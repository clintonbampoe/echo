// The number about a person's organizations for a profile. Pure, so the tests can check it without a screen.
import type { Stat } from "@/ui";

export function organizationsStat(names: string[], loading: boolean): Stat {
  const more = names.length > 3 ? ` +${names.length - 3}` : "";
  return {
    label: "Organizations",
    value: loading ? "–" : String(names.length),
    note: names.slice(0, 3).join(", ") + more,
  };
}
