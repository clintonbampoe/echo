// The four numbers on an event's page. Pure, so the tests can check them without a screen.
import { formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { EventItem } from "./api";
import type { Arrival, Guest, Phase } from "./roster";

const n = formatNumber;

// "47 of 60" with "13 spots left", or just "47" when there is no limit.
function registered(
  e: Pick<EventItem, "registrationCount" | "capacity">,
): Stat {
  const count = e.registrationCount;
  if (!e.capacity)
    return { label: "Registered", value: n(count), note: "No limit" };
  const left = Math.max(0, e.capacity - count);
  return {
    label: "Registered",
    value: `${n(count)} of ${n(e.capacity)}`,
    note:
      left === 0
        ? "No spots left"
        : `${n(left)} ${left === 1 ? "spot" : "spots"} left`,
  };
}

// Of the people who signed up, how many came.
function cameNote(
  signedUp: number,
  cameRegistered: number,
  phase: Phase,
): string {
  if (phase === "upcoming") return "Event has not started";
  return signedUp > 0
    ? `${Math.round((cameRegistered / signedUp) * 100)}% of those registered`
    : "No one registered";
}

type Rows = {
  event: Pick<EventItem, "registrationCount" | "capacity">;
  guests: Guest[];
  arrivals: Arrival[];
  phase: Phase;
};

export function rosterStats({ event, guests, arrivals, phase }: Rows): Stat[] {
  const walkIns = arrivals.filter((a) => a.walkIn).length;
  const missing = guests.filter((g) => !g.checkIn).length;
  const over = phase === "over";
  return [
    registered(event),
    {
      label: "Checked in",
      value: n(arrivals.length),
      note: cameNote(event.registrationCount, arrivals.length - walkIns, phase),
    },
    { label: "Walk-ins", value: n(walkIns), note: "Came without registering" },
    {
      label: "No-shows",
      value: over ? n(missing) : "–",
      note: over ? "Registered but did not come" : "After the event",
    },
  ];
}

// The numbers before the two lists have been read (or when one could not be): the sign-ups come from the event
// itself, and the rest wait.
export function waitingStats(
  event: Pick<EventItem, "registrationCount" | "capacity">,
  failed: boolean,
): Stat[] {
  const note = failed ? "Could not load" : "Loading";
  return [
    registered(event),
    ...["Checked in", "Walk-ins", "No-shows"].map((label) => ({
      label,
      value: "–",
      note,
    })),
  ];
}
