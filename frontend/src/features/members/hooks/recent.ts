import { useState } from "react";
import type { MemberMatch } from "../api/members";

const KEY = "echo.recent-members";
const KEEP = 5;

const isMatch = (m: unknown): m is MemberMatch =>
  typeof m === "object" &&
  m !== null &&
  typeof (m as MemberMatch).id === "string" &&
  typeof (m as MemberMatch).name === "string" &&
  typeof (m as MemberMatch).phoneNumber === "string";

// The people picked last, newest first. Kept in this browser only, and only for convenience: where storage is
// missing or unreadable there are simply no recent people.
function read(): MemberMatch[] {
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(KEY) ?? "[]");
    return Array.isArray(parsed) ? parsed.filter(isMatch).slice(0, KEEP) : [];
  } catch {
    return [];
  }
}

export function useRecentMembers() {
  const [recent, setRecent] = useState(read);
  function remember(member: MemberMatch) {
    const next = [member, ...recent.filter((m) => m.id !== member.id)].slice(
      0,
      KEEP,
    );
    setRecent(next);
    try {
      localStorage.setItem(KEY, JSON.stringify(next));
    } catch {
      // Private windows and full storage throw; the list is only a shortcut, so carry on without it.
    }
  }
  return { recent, remember };
}
