// What the "Register or check in" panel decides, apart from the screen. Pure, so the tests can check it without one.
import { isApiError } from "@/lib/api";
import type { EventItem } from "../api/events";

// What is being recorded: a sign-up, or someone arriving.
export type Mode = "register" | "check-in";

// Every spot is taken. An event with no capacity is never full.
export const isFull = (
  e: Pick<EventItem, "registrationCount" | "capacity">,
): boolean => Boolean(e.capacity) && e.registrationCount >= (e.capacity ?? 0);

// Where a person in the search stands: recorded just now from this panel, recorded before, or not yet.
export type RowState = "ready" | "already" | "done";

export function rowState(
  id: string,
  before: ReadonlySet<string>,
  now: readonly string[],
): RowState {
  if (now.includes(id)) return "done";
  return before.has(id) ? "already" : "ready";
}

// The line at the foot of the panel.
export function logText(mode: Mode, count: number): string {
  if (count === 0) return "Nothing recorded yet";
  return `${count} ${mode === "register" ? "registered" : "checked in"} just now`;
}

// The hint under the results. Registration that is closed says check-in still works; check-in says what a walk-in is.
export function footnote(
  mode: Mode,
  closed: boolean,
  matches: number | undefined,
): string | null {
  if (mode === "check-in")
    return "Not registered? You can still check them in. They show as a walk-in.";
  if (closed) return "Check in still works while registration is closed.";
  if (!matches) return null;
  return matches === 1
    ? "Showing 1 match."
    : `Showing the top ${matches} matches. Keep typing to narrow it down.`;
}

export type Problem = { title: string; message: string };

// What to tell the person when a recording was refused: the event filled up, someone else got there first, or anything else.
export function recordProblem(
  mode: Mode,
  name: string,
  error: unknown,
): Problem {
  if (isApiError(error) && error.status === 409) {
    if (error.errorCode === "EVENT_FULL")
      return {
        title: "Registration is closed",
        message:
          "All spots are taken. To register more people, raise the capacity in Edit.",
      };
    return {
      title: `${name} is already ${mode === "register" ? "registered" : "checked in"}`,
      message: "Someone else may have just recorded them.",
    };
  }
  return {
    title: `We could not ${mode === "register" ? "register" : "check in"} ${name}`,
    message: "Check your connection and try again.",
  };
}
