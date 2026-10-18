import type { EventCheckIn, EventRegistration } from "./api";
import {
  arrivals,
  eventPhase,
  filterArrivals,
  filterGuests,
  guests,
  guestStatus,
} from "./roster";

const reg = (
  id: string,
  memberId: string,
  memberName: string,
): EventRegistration => ({
  id,
  memberId,
  memberName,
  eventId: "e1",
  registrationDate: "2026-09-12",
});
const came = (
  id: string,
  memberId: string,
  memberName: string,
  checkInTime = "06:00:00",
): EventCheckIn => ({ id, memberId, memberName, eventId: "e1", checkInTime });

const ama = reg("r1", "m1", "Ama Boateng");
const comfort = reg("r2", "m2", "Comfort Addo");
const amaIn = came("c1", "m1", "Ama Boateng");
const grace = came("c2", "m9", "Grace Appiah", "06:55:00");

describe("event phase", () => {
  it("is upcoming before the first day, happening on the days, and over after the last", () => {
    const e = { startDate: "2026-10-30", endDate: "2026-11-01" };
    expect(eventPhase(e, "2026-10-29")).toBe("upcoming");
    expect(eventPhase(e, "2026-10-30")).toBe("happening");
    expect(eventPhase(e, "2026-11-01")).toBe("happening");
    expect(eventPhase(e, "2026-11-02")).toBe("over");
  });
});

describe("matching sign-ups to check-ins", () => {
  it("gives each guest their check-in, or none", () => {
    const list = guests([ama, comfort], [amaIn, grace]);
    expect(list.map((g) => guestStatus(g))).toEqual(["came", "missing"]);
    expect(list[0]?.checkIn?.id).toBe("c1");
  });

  it("calls someone who came without signing up a walk-in", () => {
    expect(
      arrivals([amaIn, grace], [ama, comfort]).map((a) => a.walkIn),
    ).toEqual([false, true]);
  });
});

describe("filtering", () => {
  const list = guests([ama, comfort], [amaIn]);
  it("finds guests by name, ignoring case, and by status", () => {
    expect(filterGuests(list, "ama", "").map((g) => g.registration.id)).toEqual(
      ["r1"],
    );
    expect(
      filterGuests(list, "", "missing").map((g) => g.registration.id),
    ).toEqual(["r2"]);
    expect(filterGuests(list, "ama", "missing")).toEqual([]);
    expect(filterGuests(list, "", "")).toHaveLength(2);
  });

  it("finds arrivals by name and by kind", () => {
    const rows = arrivals([amaIn, grace], [ama]);
    expect(
      filterArrivals(rows, "", "walk-in").map((a) => a.checkIn.id),
    ).toEqual(["c2"]);
    expect(
      filterArrivals(rows, "", "registered").map((a) => a.checkIn.id),
    ).toEqual(["c1"]);
    expect(filterArrivals(rows, "grace", "")).toHaveLength(1);
  });
});
