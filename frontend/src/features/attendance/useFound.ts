// The people a typed name finds, among members or visitors. Waits for typing to pause and for two letters.
import { useMemberSearch } from "@/features/members";
import { useVisitorSearch } from "@/features/visitors";
import { useDebounced } from "@/lib/useDebounced";
import type { PersonKind } from "./api";
import type { Match } from "./matches";

export function useFound(kind: PersonKind, typed: string) {
  const text = typed.trim();
  const query = useDebounced(text);
  const members = useMemberSearch(kind === "Member" ? query : "");
  const visitors = useVisitorSearch(kind === "Visitor" ? query : "");
  const found = kind === "Member" ? members : visitors;
  // Settled: what is shown answers what is typed now, not what was typed a moment ago.
  const settled = query === text && text.length >= 2 && !found.isPending;
  const matches: Match[] = settled ? (found.data ?? []) : [];
  return { matches, settled, failed: found.isError };
}
