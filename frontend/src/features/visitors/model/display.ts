// What the list says about a visitor. Pure, so the tests need no screen. No React in this file.
import type { Visitor } from "../api/visitors";

export type VisitorKind = "New" | "Returning";

// Came more than once, or this is the first time.
export const visitorKind = (v: Pick<Visitor, "visitCount">): VisitorKind =>
  v.visitCount > 1 ? "Returning" : "New";

// The member a visitor became, or null if they have not. The server's own link is not always filled in, but a
// member keeps the id of the visitor they were, so a visitor with a converted date is that member.
export const memberIdOf = (
  v: Pick<Visitor, "id" | "convertedAt" | "convertedToMemberPersonId">,
): string | null =>
  v.convertedToMemberPersonId ?? (v.convertedAt ? v.id : null);
