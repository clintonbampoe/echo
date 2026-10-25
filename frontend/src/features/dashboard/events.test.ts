import type { DashEvent } from "./api";
import { defaultPick, eventDays, kindOf, monthTitle } from "./events";

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

describe("monthTitle", () => {
  it("names the month", () => {
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
});
