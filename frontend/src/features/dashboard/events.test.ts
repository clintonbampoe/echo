import type { DashEvent } from "./api";
import {
  defaultPick,
  detailOf,
  eventDays,
  kindOf,
  monthTitle,
  tileOf,
  whenOf,
} from "./events";

const event = (
  name: string,
  startDate: string,
  endDate = startDate,
): DashEvent => ({
  id: name,
  name,
  startDate,
  endDate,
  registrationCount: 0,
});

const TODAY = "2026-10-19";

describe("saying it", () => {
  it("gives the day and time, or both ends of a long event", () => {
    expect(
      whenOf({
        startDate: "2026-10-24",
        endDate: "2026-10-24",
        startTime: "16:00:00",
      }),
    ).toBe("Sat 24 Oct, 4:00 pm");
    expect(whenOf({ startDate: "2026-10-30", endDate: "2026-11-01" })).toBe(
      "Fri 30 Oct to Sun 1 Nov",
    );
  });

  it("names the tile and the month", () => {
    expect(tileOf("2026-11-01")).toEqual({ month: "Nov", date: "1" });
    expect(monthTitle("2026-10")).toBe("October 2026");
  });
});

describe("eventDays", () => {
  it("marks each day of a long event, only inside the month", () => {
    const days = eventDays(
      [
        event("Rally", "2026-10-24"),
        event("Retreat", "2026-10-30", "2026-11-02"),
      ],
      "2026-10",
    );
    expect([...days.keys()]).toEqual([
      "2026-10-24",
      "2026-10-30",
      "2026-10-31",
    ]);
  });

  it("lists every event on a day that has two", () => {
    const days = eventDays(
      [event("A", "2026-10-24"), event("B", "2026-10-24")],
      "2026-10",
    );
    expect(days.get("2026-10-24")).toEqual(["A", "B"]);
  });
});

describe("which day, and what it says", () => {
  const days = eventDays(
    [event("Done", "2026-10-03"), event("Rally", "2026-10-24")],
    "2026-10",
  );

  it("counts today as upcoming", () => {
    expect(kindOf("2026-10-19", TODAY)).toBe("upcoming");
    expect(kindOf("2026-10-18", TODAY)).toBe("held");
  });

  it("starts on the next event, or the last one when they are all over", () => {
    expect(defaultPick(days, TODAY)).toBe("2026-10-24");
    expect(defaultPick(days, "2026-11-30")).toBe("2026-10-24");
    expect(defaultPick(new Map(), TODAY)).toBeUndefined();
  });

  it("writes the line beside the legend", () => {
    expect(detailOf("2026-10-24", days, true)).toBe("Sat 24 Oct, Rally");
    expect(detailOf(undefined, new Map(), true)).toBe("No events this month");
    expect(detailOf(undefined, new Map(), false)).toBe("\u00a0");
    const two = new Map([["2026-10-24", ["A", "B", "C"]]]);
    expect(detailOf("2026-10-24", two, true)).toBe("Sat 24 Oct, A and 2 more");
  });
});
