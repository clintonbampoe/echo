import type { EventRoster } from "../api/checkIns";
import { rosterStats, waitingStats } from "./rosterFigures";

// Three came: two of the four who signed up, and one walk-in.
const roster: EventRoster = { checkedIn: 3, walkIns: 1, cameRegistered: 2 };
const rows = (
  event: { registrationCount: number; capacity?: number | null },
  phase: "upcoming" | "happening" | "over",
) => rosterStats({ event, roster, phase });

describe("event numbers", () => {
  it("counts who signed up, who came, the show rate and the walk-ins", () => {
    const stats = rows({ registrationCount: 4, capacity: 60 }, "over");
    expect(stats.map((s) => [s.label, s.value])).toEqual([
      ["Registered", "4"],
      ["Checked in", "3"],
      ["Show rate", "50"],
      ["Walk-ins", "1"],
    ]);
    expect(stats[0]?.note).toBe("56 spots left of 60");
    expect(stats[1]?.note).toBe("50% of those registered");
    expect(stats[2]?.unit).toBe("%");
  });

  it("says there is no limit, and when no spots are left", () => {
    expect(
      rows({ registrationCount: 4, capacity: null }, "over")[0],
    ).toMatchObject({ value: "4", note: "No limit" });
    expect(rows({ registrationCount: 4, capacity: 4 }, "over")[0]?.note).toBe(
      "All 4 spots taken",
    );
    expect(rows({ registrationCount: 59, capacity: 60 }, "over")[0]?.note).toBe(
      "1 spot left of 60",
    );
  });

  it("has no show rate before the event, or with no sign-ups", () => {
    expect(
      rows({ registrationCount: 4, capacity: 60 }, "upcoming")[2],
    ).toMatchObject({ value: "–", note: "After the event" });
    expect(rows({ registrationCount: 0, capacity: 60 }, "over")[2]?.value).toBe(
      "–",
    );
    expect(
      rows({ registrationCount: 4, capacity: 60 }, "upcoming")[1]?.note,
    ).toBe("Event has not started");
  });

  it("shows waiting numbers while the roster is counted, or when it could not be", () => {
    expect(
      waitingStats({ registrationCount: 4, capacity: 60 }, false).map(
        (s) => s.value,
      ),
    ).toEqual(["4", "–", "–", "–"]);
    expect(
      waitingStats({ registrationCount: 4, capacity: 60 }, true)[1]?.note,
    ).toBe("Could not load");
  });
});
