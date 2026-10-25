// Who signed up for an event and who came, matched up. The server lists the two separately, so this joins them by
// member. Pure, so the tests can check it without a screen.
import type { EventCheckIn } from "../api/checkIns";
import type { EventItem } from "../api/events";
import type { EventRegistration } from "../api/registrations";

// Where an event is: still to come, on now (its first day has begun and its last has not ended), or over.
export type Phase = "upcoming" | "happening" | "over";

export function eventPhase(
  e: Pick<EventItem, "startDate" | "endDate">,
  today: string,
): Phase {
  if (e.startDate.slice(0, 10) > today) return "upcoming";
  return e.endDate.slice(0, 10) < today ? "over" : "happening";
}

// Someone who signed up, and when they came, if they did.
export type Guest = { registration: EventRegistration; checkIn?: EventCheckIn };

// Someone who came, and whether they had signed up. A walk-in came without registering.
export type Arrival = { checkIn: EventCheckIn; walkIn: boolean };

export function guests(
  registrations: EventRegistration[],
  checkIns: EventCheckIn[],
): Guest[] {
  const came = new Map(checkIns.map((c) => [c.memberId, c]));
  return registrations.map((registration) => ({
    registration,
    checkIn: came.get(registration.memberId),
  }));
}

export function arrivals(
  checkIns: EventCheckIn[],
  registrations: EventRegistration[],
): Arrival[] {
  const signedUp = new Set(registrations.map((r) => r.memberId));
  return checkIns.map((checkIn) => ({
    checkIn,
    walkIn: !signedUp.has(checkIn.memberId),
  }));
}

// How a guest stands, as the words on their row: checked in, still to come, or gone without coming.
export type GuestStatus = "came" | "missing";

export const guestStatus = (g: Guest): GuestStatus =>
  g.checkIn ? "came" : "missing";

const has = (name: string, text: string) =>
  name.toLowerCase().includes(text.trim().toLowerCase());

// The guests whose name has what was typed in it and who are in the chosen state ('' is any).
export const filterGuests = (
  list: Guest[],
  name: string,
  status: string,
): Guest[] =>
  list.filter(
    (g) =>
      has(g.registration.memberName, name) &&
      (status === "" || guestStatus(g) === status),
  );

// The arrivals whose name has what was typed in it and who are the chosen kind: 'registered', 'walk-in' or '' for any.
export const filterArrivals = (
  list: Arrival[],
  name: string,
  kind: string,
): Arrival[] =>
  list.filter(
    (a) =>
      has(a.checkIn.memberName, name) &&
      (kind === "" || (kind === "walk-in") === a.walkIn),
  );
