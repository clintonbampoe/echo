// The rows of the Members card's status column. Pure.
import type { MemberMix } from "./api";

export type StatusRow = { key: string; name: string; count: number };

// Active, inactive (everyone else), and how many joined this year. Nothing while the numbers are on their way.
export function statusRows(
  mix: MemberMix | undefined,
  joinedThisYear: number | undefined,
): StatusRow[] {
  if (!mix) return [];
  const rows = [
    { key: "active", name: "Active", count: mix.active },
    {
      key: "inactive",
      name: "Inactive",
      count: Math.max(0, mix.total - mix.active),
    },
  ];
  return joinedThisYear === undefined
    ? rows
    : [...rows, { key: "new", name: "New this year", count: joinedThisYear }];
}
