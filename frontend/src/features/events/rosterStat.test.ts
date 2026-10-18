import type { EventCheckIn, EventRegistration } from "./api";
import { arrivals, guests } from "./roster";
import { rosterStats, waitingStats } from "./rosterStat";

const reg = (id: string, memberId: string): EventRegistration => ({
  id,
  memberId,
  memberName: id,
  eventId: "e1",
  registrationDate: "2026-09-12",
});
const came = (id: string, memberId: string): EventCheckIn => ({
  id,
  memberId,
  memberName: id,
  eventId: "e1",
  checkInTime: "06:00:00",
});

const regs = [reg("a", "m1"), reg("b", "m2"), reg("c", "m3"), reg("d", "m4")];
const checks = [came("x", "m1"), came("y", "m2"), came("z", "m9")];
const rows = (
  event: { registrationCount: number; capacity?: number | null },
  phase: "upcoming" | "happening" | "over",
) =>
  rosterStats({
    event,
    guests: guests(regs, checks),
    arrivals: arrivals(checks, regs),
    phase,
  });

describe("event numbers", () => {
  it("counts registered against the capacity, who came, the walk-ins and the no-shows once it is over", () => {
    const stats = rows({ registrationCount: 4, capacity: 60 }, "over");
    expect(stats.map((s) => s.value)).toEqual(["4 of 60", "3", "1", "2"]);
    expect(stats[0]?.note).toBe("56 spots left");
    expect(stats[1]?.note).toBe("50% of those registered");
    expect(stats[3]?.note).toBe("Registered but did not come");
  });

  it("says there is no limit, and when no spots are left", () => {
    expect(
      rows({ registrationCount: 4, capacity: null }, "over")[0],
    ).toMatchObject({ value: "4", note: "No limit" });
    expect(rows({ registrationCount: 4, capacity: 4 }, "over")[0]?.note).toBe(
      "No spots left",
    );
    expect(rows({ registrationCount: 59, capacity: 60 }, "over")[0]?.note).toBe(
      "1 spot left",
    );
  });

  it("does not call anyone a no-show before the event is over", () => {
    const stats = rows({ registrationCount: 4, capacity: 60 }, "happening");
    expect(stats[3]).toMatchObject({ value: "–", note: "After the event" });
    expect(
      rows({ registrationCount: 4, capacity: 60 }, "upcoming")[1]?.note,
    ).toBe("Event has not started");
  });

  it("shows waiting numbers while the lists load, or when they could not", () => {
    expect(
      waitingStats({ registrationCount: 4, capacity: 60 }, false).map(
        (s) => s.value,
      ),
    ).toEqual(["4 of 60", "–", "–", "–"]);
    expect(
      waitingStats({ registrationCount: 4, capacity: 60 }, true)[1]?.note,
    ).toBe("Could not load");
  });
});
