import type { EventItem } from "./api";
import { capacityShare, eventSub, fillOf } from "./rows";

const event = (over: Partial<EventItem> = {}): EventItem =>
  ({
    registrationCount: 22,
    capacity: 60,
    ...over,
  }) as EventItem;

describe("event rows", () => {
  it("says the day, and the place when there is one", () => {
    const e = { startDate: "2026-10-17", endDate: "2026-10-17" };
    expect(eventSub({ ...e, location: "Main Auditorium" })).toBe(
      "Sat 17 Oct · Main Auditorium",
    );
    expect(eventSub({ ...e, location: null })).toBe("Sat 17 Oct");
  });

  it("gives the first and last day of an event over several days", () => {
    expect(
      eventSub({
        startDate: "2026-10-30",
        endDate: "2026-11-01",
        location: "Annex",
      }),
    ).toBe("Fri 30 Oct to Sun 1 Nov · Annex");
  });

  it("says how full an event is, never past full", () => {
    expect(capacityShare(75, 150)).toBe(0.5);
    expect(capacityShare(160, 150)).toBe(1);
    expect(capacityShare(12, null)).toBeNull();
    expect(capacityShare(12, 0)).toBeNull();
  });

  it("says how much of the room an upcoming event has taken", () => {
    expect(fillOf(event(), false)).toEqual({
      label: "Registered",
      text: "37% of 60",
      share: 22 / 60,
    });
  });

  it("says how many of those registered came to a past event, when it knows", () => {
    expect(
      fillOf(event({ attendanceCount: 20, registrationCount: 25 }), true),
    ).toEqual({ label: "Came", text: "80% of 25", share: 0.8 });
    expect(fillOf(event(), true).label).toBe("Registered");
  });

  it("gives just the count when there is no limit", () => {
    expect(fillOf(event({ capacity: undefined }), false).text).toBe(
      "22 · No limit",
    );
  });
});
