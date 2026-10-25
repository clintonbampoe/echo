// Marking people present at one session: what was marked from this screen, the undo, and the visitor who is
// added and marked in one go. Talks to the server only through hooks.ts.
import { useState } from "react";
import { useCreateVisitor } from "@/features/visitors";
import { isApiError } from "@/lib/api";
import { useCheckIn, useRemoveCheckIn } from ".";
import type { Match } from "../model/matches";

export type Marked = { checkInId: string; personId: string; name: string };
export type NewName = { firstName: string; lastName: string };

// `onSettled` runs after every try, so the panel can empty its search and keep the keyboard where it was.
export function useMarking(sessionId: string, onSettled: () => void) {
  const check = useCheckIn(sessionId);
  const remove = useRemoveCheckIn();
  const createVisitor = useCreateVisitor();
  const [marked, setMarked] = useState<Marked[]>([]);
  // People found to be on the list already, beyond the page of check-ins that has loaded.
  const [known, setKnown] = useState<ReadonlySet<string>>(new Set());
  const [failed, setFailed] = useState(false);

  async function mark(person: Match) {
    setFailed(false);
    try {
      const saved = await check.mutateAsync(person.id);
      setMarked((all) => [
        ...all,
        { checkInId: saved.id, personId: person.id, name: person.name },
      ]);
    } catch (e) {
      // Someone else marked them first: they are on the list either way.
      if (isApiError(e) && e.status === 409)
        setKnown((all) => new Set([...all, person.id]));
      else setFailed(true);
    }
    onSettled();
  }

  // Everyone in the list, one after the other, so each is saved (or found to be here already) on its own.
  async function markAll(people: Match[]) {
    for (const person of people) await mark(person);
  }

  async function addVisitor(name: NewName) {
    setFailed(false);
    try {
      const visitor = await createVisitor.mutateAsync(name);
      await mark({ id: visitor.id, name: visitor.name });
    } catch {
      setFailed(true);
    }
  }

  async function undo() {
    const last = marked[marked.length - 1];
    if (!last) return;
    try {
      await remove.mutateAsync(last.checkInId);
      setMarked((all) => all.slice(0, -1));
    } catch {
      setFailed(true);
    }
  }

  return {
    marked,
    known,
    failed,
    busy: check.isPending || createVisitor.isPending,
    undoing: remove.isPending,
    mark,
    markAll,
    addVisitor,
    undo,
  };
}
