// What the event form accepts, and the words shown when it does not. The server has the final say; this catches
// the obvious mistakes before a round trip. No React in this file.
import { z } from "zod";
import { required } from "@/lib/validation";
import type { EventInput, EventItem } from "./api";

export const NAME_MAX = 100;
export const LOCATION_MAX = 255;
export const DESCRIPTION_MAX = 2000;
export const CAPACITY_MAX = 100_000;

// A time or a limit already set can be changed, not taken away: the API has no way to clear them.
export type Locked = {
  startTime: boolean;
  endTime: boolean;
  capacity: boolean;
};
const NONE: Locked = { startTime: false, endTime: false, capacity: false };

const isCapacity = (text: string) => {
  const t = text.trim();
  return (
    t === "" || (/^\d+$/.test(t) && Number(t) >= 1 && Number(t) <= CAPACITY_MAX)
  );
};

type Say = (path: string, message: string) => void;
type Times = {
  startDate: string;
  endDate: string;
  startTime: string;
  endTime: string;
  capacity: string;
};

// The end cannot come before the start, by day or, on a one-day event, by time.
function checkOrder(v: Times, say: Say) {
  if (v.startDate && v.endDate && v.endDate < v.startDate)
    say("endDate", "The end date cannot be before the start date");
  const sameDay = v.startDate === v.endDate;
  if (sameDay && v.startTime && v.endTime && v.endTime <= v.startTime)
    say("endTime", "The end must be after the start");
}

// What is already set stays set.
function checkLocked(v: Times, locked: Locked, say: Say) {
  if (locked.startTime && !v.startTime)
    say("startTime", "A time can be changed, not removed");
  if (locked.endTime && !v.endTime)
    say("endTime", "A time can be changed, not removed");
  if (locked.capacity && !v.capacity.trim())
    say("capacity", "A limit can be changed, not removed");
}

export const eventSchema = (locked: Locked = NONE) =>
  z
    .object({
      name: required("Enter a name").max(
        NAME_MAX,
        `Use ${NAME_MAX} characters or fewer`,
      ),
      organizationId: z.string().min(1, "Choose a group"),
      organizerId: z.string().min(1, "Choose an organizer"),
      startDate: z.string().min(1, "Enter the start date"),
      endDate: z.string().min(1, "Enter the end date"),
      startTime: z.string(),
      endTime: z.string(),
      location: z
        .string()
        .max(LOCATION_MAX, `Use ${LOCATION_MAX} characters or fewer`),
      capacity: z
        .string()
        .refine(isCapacity, "Enter a whole number from 1 to 100,000"),
      description: z
        .string()
        .max(DESCRIPTION_MAX, `Use ${DESCRIPTION_MAX} characters or fewer`),
    })
    .superRefine((v, ctx) => {
      const say = (path: string, message: string) =>
        ctx.addIssue({ code: "custom", path: [path], message });
      checkOrder(v, say);
      checkLocked(v, locked, say);
    });

export type EventValues = z.infer<ReturnType<typeof eventSchema>>;

export const emptyEvent: EventValues = {
  name: "",
  organizationId: "",
  organizerId: "",
  startDate: "",
  endDate: "",
  startTime: "",
  endTime: "",
  location: "",
  capacity: "",
  description: "",
};

// "08:00:00" in the API, "08:00" in a time box.
const toBox = (time?: string | null) => (time ?? "").slice(0, 5);
const toApi = (time: string) => (time ? `${time}:00` : undefined);

export function fromEvent(e: EventItem): EventValues {
  return {
    name: e.name,
    organizationId: e.organizationId,
    organizerId: e.organizerId,
    startDate: e.startDate.slice(0, 10),
    endDate: e.endDate.slice(0, 10),
    startTime: toBox(e.startTime),
    endTime: toBox(e.endTime),
    location: e.location ?? "",
    capacity: e.capacity ? String(e.capacity) : "",
    description: e.description ?? "",
  };
}

export const lockedFor = (
  e: Pick<EventItem, "startTime" | "endTime" | "capacity">,
): Locked => ({
  startTime: Boolean(e.startTime),
  endTime: Boolean(e.endTime),
  capacity: Boolean(e.capacity),
});

// Adding: what was left empty is left out.
export function toEventInput(v: EventValues): EventInput {
  return {
    organizationId: v.organizationId,
    organizerId: v.organizerId,
    name: v.name.trim(),
    startDate: v.startDate,
    endDate: v.endDate,
    startTime: toApi(v.startTime),
    endTime: toApi(v.endTime),
    location: v.location.trim() || undefined,
    capacity: v.capacity.trim() ? Number(v.capacity) : undefined,
    description: v.description.trim() || undefined,
  };
}

// Saving changes: text is cleared by sending it empty, so the place and the description always go.
export const toEventUpdate = (v: EventValues): Partial<EventInput> => ({
  ...toEventInput(v),
  location: v.location.trim(),
  description: v.description.trim(),
});

export const eventFieldMap = Object.fromEntries(
  Object.keys(emptyEvent).map((k) => [k, k]),
) as Record<keyof EventValues, keyof EventValues>;
