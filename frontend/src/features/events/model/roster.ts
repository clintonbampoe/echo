// Who signed up for an event and who came. The server lists each with the other's answer in it (a registration says
// whether the person came, a check-in whether they were a walk-in), so there is nothing to match up here.
import type { CheckInFilters, EventCheckIn } from "../api/checkIns";
import type { EventItem } from "../api/events";
import type {
  EventRegistration,
  RegistrationFilters,
} from "../api/registrations";

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

// A registration as the server lists it for one event, which already says whether the person came.
export const toGuest = (registration: EventRegistration): Guest => ({
  registration,
  checkIn: registration.checkIn
    ? {
        id: registration.checkIn.id,
        memberId: registration.memberId,
        memberName: registration.memberName,
        eventId: registration.eventId,
        checkInTime: registration.checkIn.checkInTime,
      }
    : undefined,
});

// A check-in as the server lists it for one event, which already says whether the person was a walk-in.
export const toArrival = (checkIn: EventCheckIn): Arrival => ({
  checkIn,
  walkIn: checkIn.walkIn === true,
});

// How a guest stands, as the words on their row: checked in, still to come, or gone without coming.
export type GuestStatus = "came" | "missing";

export const guestStatus = (g: Guest): GuestStatus =>
  g.checkIn ? "came" : "missing";

// What the Registrations tab's search box and status choice ask the server for ('' is any).
export const registrationFilters = (
  name: string,
  status: string,
): RegistrationFilters => ({
  name: name.trim() || undefined,
  status:
    status === "came" ? "Came" : status === "missing" ? "Missing" : undefined,
});

// What the Check-ins tab's search box and type choice ask the server for ('' is any).
export const checkInFilters = (name: string, kind: string): CheckInFilters => ({
  name: name.trim() || undefined,
  kind:
    kind === "registered"
      ? "Registered"
      : kind === "walk-in"
        ? "WalkIn"
        : undefined,
});
