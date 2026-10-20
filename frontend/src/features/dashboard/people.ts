// What the visitor, gender and age cards say. Pure.
import { formatAgo } from "@/lib/format";
import type {
  GenderFigures,
  MemberMix,
  RecentVisitor,
  VisitorFigures,
} from "./api";
import type { Row } from "./shares";

// The line under the number of visitors who became members. The number sits on its own, so the line
// does not repeat it. Something there while the count is on its way.
export const joinedLabel = (joined?: number) =>
  joined === undefined
    ? "Visitors who became members"
    : joined === 1
      ? "Visitor became a member"
      : "Visitors became members";

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

// The age groups, in the dashboard's own words. The server labels its bands with dashes ("18–29"); the
// paper asks for spoken words, and the five bands always come in this order.
const AGE_WORDS = [
  "Under 18",
  "18 to 29",
  "30 to 44",
  "45 to 59",
  "60 and over",
] as const;

export type AgeRow = { key: string; name: string; count: number };

export function ageRows(mix: MemberMix | undefined): AgeRow[] {
  if (!mix) return [];
  return mix.bands.map((b, i) => ({
    key: b.label,
    name: AGE_WORDS[i] ?? b.label,
    count: b.count,
  }));
}

// The average age as the card says it: "38". Nothing to say for a church with no members.
export const averageAge = (mix: MemberMix | undefined) =>
  mix && mix.total > 0 ? String(Math.round(mix.average)) : undefined;

// "Added yesterday", or why there is nothing to say. The clock can be a moment ahead of us, and a day
// after today is still today as far as the words go.
export const addedAgo = (visitor: RecentVisitor, today: string): string => {
  if (visitor.added >= today) return "Added today";
  return `Added ${formatAgo(visitor.added, today).toLowerCase()}`;
};
