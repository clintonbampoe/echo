// People the check-in panel can find, from either kind of search. No React in this file.
import type { PersonKind } from "../api/checkIns";

export type Match = { id: string; name: string; phoneNumber?: string | null };

// Where a found person stands: marked a moment ago from this panel, already on the list, or not yet.
export type Standing = "now" | "already" | "none";

export function standing(
  id: string,
  now: ReadonlySet<string>,
  already: ReadonlySet<string>,
): Standing {
  if (now.has(id)) return "now";
  return already.has(id) ? "already" : "none";
}

export const kindOptions: readonly { value: PersonKind; label: string }[] = [
  { value: "Member", label: "Member" },
  { value: "Visitor", label: "Visitor" },
];

// "Ama Serwaa Boateng" -> first "Ama", last "Serwaa Boateng". One word is not enough to add a visitor.
export function splitName(
  text: string,
): { firstName: string; lastName: string } | null {
  const [first, ...rest] = text.trim().split(/\s+/);
  return first && rest.length > 0
    ? { firstName: first, lastName: rest.join(" ") }
    : null;
}
