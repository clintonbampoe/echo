import type { EventItem } from "../api/events";
import {
  emptyEvent,
  eventSchema,
  fromEvent,
  lockedFor,
  toEventInput,
  toEventUpdate,
  type EventValues,
} from "./schema";

const good: EventValues = {
  ...emptyEvent,
  name: " Annual Convention ",
  organizationId: "o1",
  organizerId: "m1",
  startDate: "2026-10-30",
  endDate: "2026-11-01",
};
const issues = (v: EventValues, locked?: Parameters<typeof eventSchema>[0]) => {
  const r = eventSchema(locked).safeParse(v);
  return r.success
    ? {}
    : Object.fromEntries(
        r.error.issues.map((i) => [String(i.path[0]), i.message]),
      );
};

describe("event form", () => {
  it("asks for the four things the event cannot do without", () => {
    expect(Object.keys(issues(emptyEvent)).sort()).toEqual([
      "endDate",
      "name",
      "organizationId",
      "organizerId",
      "startDate",
    ]);
    expect(issues(good)).toEqual({});
  });

  it("does not let the event end before it starts", () => {
    expect(issues({ ...good, endDate: "2026-10-29" }).endDate).toBe(
      "The end date cannot be before the start date",
    );
  });

  it("checks the times only on a one-day event", () => {
    const day = {
      ...good,
      endDate: good.startDate,
      startTime: "10:00",
      endTime: "09:00",
    };
    expect(issues(day).endTime).toBe("The end must be after the start");
    expect(issues({ ...day, endDate: "2026-11-01" })).toEqual({});
  });

  it("takes a capacity that is empty or a whole number from 1 to 100,000", () => {
    for (const ok of ["", "1", " 150 ", "100000"])
      expect(issues({ ...good, capacity: ok }).capacity).toBeUndefined();
    for (const bad of ["0", "-5", "1.5", "many", "100001"])
      expect(issues({ ...good, capacity: bad }).capacity).toBeDefined();
  });

  it("does not let a time or the limit that is set be taken away", () => {
    const locked = { startTime: true, endTime: false, capacity: true };
    const found = issues(good, locked);
    expect(found.startTime).toBe("A time can be changed, not removed");
    expect(found.capacity).toBe("A limit can be changed, not removed");
    expect(found.endTime).toBeUndefined();
  });

  it("sends only what was filled in when adding", () => {
    expect(
      toEventInput({
        ...good,
        startTime: "16:00",
        capacity: "150",
        location: " Hall ",
      }),
    ).toEqual({
      organizationId: "o1",
      organizerId: "m1",
      name: "Annual Convention",
      startDate: "2026-10-30",
      endDate: "2026-11-01",
      startTime: "16:00:00",
      endTime: undefined,
      location: "Hall",
      capacity: 150,
      description: undefined,
    });
  });

  it("sends the place and the description empty when changing, so they can be cleared", () => {
    const update = toEventUpdate(good);
    expect(update.location).toBe("");
    expect(update.description).toBe("");
    expect(update.startTime).toBeUndefined();
  });

  it("fills the form from an event, and says which parts are locked", () => {
    const e = {
      id: "e1",
      name: "Rally",
      organizationId: "o1",
      organizerId: "m1",
      startDate: "2026-10-17T00:00:00",
      endDate: "2026-10-17",
      startTime: "16:00:00",
      endTime: null,
      location: null,
      capacity: 150,
      description: null,
    } as EventItem;
    expect(fromEvent(e)).toMatchObject({
      startDate: "2026-10-17",
      startTime: "16:00",
      endTime: "",
      location: "",
      capacity: "150",
      description: "",
    });
    expect(lockedFor(e)).toEqual({
      startTime: true,
      endTime: false,
      capacity: true,
    });
  });
});
