// What the visitor and gender cards say. Pure.
import { plural } from "@/lib/format";
import type { GenderFigures, VisitorFigures } from "./api";
import type { Row } from "./shares";

// The line under the number of visitors who became members.
export const joinedLabel = (joined?: number) =>
  joined === undefined
    ? "Visitors who became members"
    : plural(joined, "Visitor became a member", "Visitors became members");

export type VisitorRow = {
  key: string;
  name: string;
  value?: number;
  // How full the bar is, 0 to 1: each row against everyone added.
  share: number;
};

// New visitors, those who became members, and those still visiting (the difference, never below zero).
export function visitorRows(f: VisitorFigures | undefined): VisitorRow[] {
  const added = f?.added ?? 0;
  const joined = f?.joined ?? 0;
  const share = (n: number) => (added > 0 ? Math.min(1, n / added) : 0);
  return [
    {
      key: "new",
      name: "New visitors",
      value: f && added,
      share: share(added),
    },
    {
      key: "joined",
      name: "Became members",
      value: f && joined,
      share: share(joined),
    },
    {
      key: "still",
      name: "Still visiting",
      value: f && Math.max(0, added - joined),
      share: share(Math.max(0, added - joined)),
    },
  ];
}

// Men, women and everyone else (the rest of the total).
export function genderRows(f: GenderFigures | undefined): Row[] {
  if (!f) return [];
  return [
    { key: "male", name: "Male", total: f.male },
    { key: "female", name: "Female", total: f.female },
    {
      key: "other",
      name: "Other",
      total: Math.max(0, f.total - f.male - f.female),
    },
  ];
}
