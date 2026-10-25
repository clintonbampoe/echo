// The four numbers on an event's page. Pure, so the tests can check them without a screen.
import { formatNumber } from "@/lib/format";
import type { Stat } from "@/ui";
import type { EventItem } from "../api/events";
import type { Arrival, Phase } from "./roster";

const n = formatNumber;

// How many signed up, with how many spots are left when there is a limit.
function registered(
  e: Pick<EventItem, "registrationCount" | "capacity">,
): Stat {
  const count = e.registrationCount;
  if (!e.capacity)
    return { label: "Registered", value: n(count), note: "No limit" };
  const left = Math.max(0, e.capacity - count);
  return {
    label: "Registered",
    value: n(count),
    note:
      left === 0
        ? `All ${n(e.capacity)} spots taken`
        : `${n(left)} ${left === 1 ? "spot" : "spots"} left of ${n(e.capacity)}`,
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
  arrivals: Arrival[];
  phase: Phase;
};

// Of those who signed up, the share who came, as a number. Nothing before the event, or with no sign-ups.
function showRate(signedUp: number, came: number, phase: Phase): Stat {
  const known = phase !== "upcoming" && signedUp > 0;
  return {
    label: "Show rate",
    value: known ? String(Math.round((came / signedUp) * 100)) : "–",
    unit: known ? "%" : undefined,
    note: known ? "Of those registered" : "After the event",
  };
}

export function rosterStats({ event, arrivals, phase }: Rows): Stat[] {
  const walkIns = arrivals.filter((a) => a.walkIn).length;
  const came = arrivals.length - walkIns;
  return [
    registered(event),
    {
      label: "Checked in",
      value: n(arrivals.length),
      note: cameNote(event.registrationCount, came, phase),
    },
    showRate(event.registrationCount, came, phase),
    { label: "Walk-ins", value: n(walkIns), note: "Came without registering" },
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
    ...["Checked in", "Show rate", "Walk-ins"].map((label) => ({
      label,
      value: "–",
      note,
    })),
  ];
}
