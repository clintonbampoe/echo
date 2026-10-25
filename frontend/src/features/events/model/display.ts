// Where a person stands with an event they signed up for. Pure, so the tests can pass "today".
export type EventStatus = "attended" | "upcoming" | "missed";

// Checked in is attended, whenever the event was. Otherwise it is still to come, or it passed without them.
export function eventStatus(
  startDate: string,
  checkedIn: boolean,
  today: string,
): EventStatus {
  if (checkedIn) return "attended";
  return startDate.slice(0, 10) >= today ? "upcoming" : "missed";
}
