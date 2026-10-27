// What a person's panel on an event shows. Pure, so the tests can check it without a screen.
import type { Arrival, Guest, Phase } from "./roster";

// One person's part in this event: whether they signed up, and when they arrived. Either half may be
// missing — someone registered who never came, or a walk-in who came without registering.
// `registered` is the fact; `registration` is the record, whose id only the Registrations tab knows.
export type EventPerson = {
  memberId: string;
  name: string;
  registered: boolean;
  registration?: { id: string; registrationDate: string };
  checkIn?: { id: string; checkInTime: string };
};

export const guestPerson = (g: Guest): EventPerson => ({
  memberId: g.registration.memberId,
  name: g.registration.memberName,
  registered: true,
  registration: {
    id: g.registration.id,
    registrationDate: g.registration.registrationDate,
  },
  checkIn: g.checkIn && {
    id: g.checkIn.id,
    checkInTime: g.checkIn.checkInTime,
  },
});

// A check-in row carries no sign-up id, so from the Check-ins tab the panel can say they registered but not
// undo it. Removing a registration belongs on the tab that lists registrations.
export const arrivalPerson = (a: Arrival): EventPerson => ({
  memberId: a.checkIn.memberId,
  name: a.checkIn.memberName,
  registered: !a.walkIn,
  checkIn: { id: a.checkIn.id, checkInTime: a.checkIn.checkInTime },
});

// The panel is named after the record it is mainly about, so one person gets one title whichever tab they
// were opened from.
export const panelTitle = (p: EventPerson): string =>
  p.registered ? "Registration" : "Check-in";

// What the panel says when they have not arrived: only an event that has ended has no-shows.
export const missingText = (phase: Phase): string =>
  phase === "over" ? "Did not come" : "Not checked in yet";

// The Signed up line. A walk-in never signed up; from the Check-ins tab we know they did but not when.
export const signedUpText = (
  p: EventPerson,
  when: (day: string) => string,
): string => {
  if (!p.registered) return "Came without registering";
  return p.registration ? when(p.registration.registrationDate) : "Yes";
};
