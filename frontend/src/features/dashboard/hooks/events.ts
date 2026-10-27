import { ALL_TIME } from "@/lib/dates";
import { getEvents } from "../api/events";
import { useData } from "./internals";

const NEXT_EVENTS = 5;

// The next few events that have not started yet, soonest first.
export function useUpcomingEvents(today: string) {
  const { data, failed, retry } = useData(
    "events-next",
    today,
    ALL_TIME.to,
    (from, to, signal) => getEvents(from, to, "Asc", NEXT_EVENTS, signal),
  );
  return { events: data, failed, retry };
}

// Every event that starts in a range, up to the server's page of 50.
export function useEventsBetween(from: string, to: string) {
  const { data, failed, retry } = useData(
    "events-in",
    from,
    to,
    (f, t, signal) => getEvents(f, t, "Asc", 50, signal),
  );
  return { events: data, failed, retry };
}
