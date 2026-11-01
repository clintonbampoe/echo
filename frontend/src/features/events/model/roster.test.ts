import type { EventCheckIn } from "../api/checkIns";
import type { EventRegistration } from "../api/registrations";
import {
  checkInFilters,
  eventPhase,
  guestStatus,
  registrationFilters,
  toArrival,
  toGuest,
} from "./roster";

const reg = (over: Partial<EventRegistration> = {}): EventRegistration => ({
  id: "r1",
  memberId: "m1",
  memberName: "Ama Boateng",
  eventId: "e1",
  registrationDate: "2026-09-12",
  ...over,
});
const came = (over: Partial<EventCheckIn> = {}): EventCheckIn => ({
  id: "c1",
  memberId: "m1",
  memberName: "Ama Boateng",
  eventId: "e1",
  checkInTime: "06:00:00",
  ...over,
});

describe("event phase", () => {
  it("is upcoming before the first day, happening on the days, and over after the last", () => {
    const e = { startDate: "2026-10-30", endDate: "2026-11-01" };
    expect(eventPhase(e, "2026-10-29")).toBe("upcoming");
    expect(eventPhase(e, "2026-10-30")).toBe("happening");
    expect(eventPhase(e, "2026-11-01")).toBe("happening");
    expect(eventPhase(e, "2026-11-02")).toBe("over");
  });
});

describe("a registration, as the server lists it for one event", () => {
  it("carries the check-in of someone who came", () => {
    const guest = toGuest(
      reg({ checkIn: { id: "c1", checkInTime: "06:05:00" } }),
    );
    expect(guestStatus(guest)).toBe("came");
    expect(guest.checkIn).toMatchObject({
      id: "c1",
      memberId: "m1",
      memberName: "Ama Boateng",
      checkInTime: "06:05:00",
    });
  });

  it("has no check-in for someone who did not come", () => {
    const guest = toGuest(reg({ checkIn: null }));
    expect(guestStatus(guest)).toBe("missing");
    expect(guest.checkIn).toBeUndefined();
  });
});

describe("a check-in, as the server lists it for one event", () => {
  it("is a walk-in only when the server says so", () => {
    expect(toArrival(came({ walkIn: true })).walkIn).toBe(true);
    expect(toArrival(came({ walkIn: false })).walkIn).toBe(false);
    expect(toArrival(came()).walkIn).toBe(false);
  });
});

describe("what the tabs ask the server for", () => {
  it("sends a trimmed name and the status, and leaves out what is empty", () => {
    expect(registrationFilters("  ama ", "missing")).toEqual({
      name: "ama",
      status: "Missing",
    });
    expect(registrationFilters("", "came")).toEqual({
      name: undefined,
      status: "Came",
    });
    expect(registrationFilters("", "")).toEqual({
      name: undefined,
      status: undefined,
    });
  });

  it("sends the check-in type the way the API names it", () => {
    expect(checkInFilters("grace", "walk-in")).toEqual({
      name: "grace",
      kind: "WalkIn",
    });
    expect(checkInFilters("", "registered").kind).toBe("Registered");
    expect(checkInFilters("", "").kind).toBeUndefined();
  });
});
