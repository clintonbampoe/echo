import {
  capacityLabel,
  capacityShare,
  dateTile,
  eventWhen,
  eventWhere,
} from "./rows";

describe("event rows", () => {
  it("says when a one-day event happens, with its time", () => {
    expect(
      eventWhen({
        startDate: "2026-10-17",
        endDate: "2026-10-17",
        startTime: "16:00:00",
      }),
    ).toBe("Sat 17 Oct, 4:00 pm");
    expect(eventWhen({ startDate: "2026-10-17", endDate: "2026-10-17" })).toBe(
      "Sat 17 Oct",
    );
  });

  it("gives the first and last day of an event over several days", () => {
    expect(
      eventWhen({
        startDate: "2026-10-30",
        endDate: "2026-11-01",
        startTime: "08:00:00",
      }),
    ).toBe("Fri 30 Oct to Sun 1 Nov");
  });

  it("adds the place when there is one", () => {
    const e = {
      startDate: "2026-10-17",
      endDate: "2026-10-17",
      startTime: "16:00:00",
    };
    expect(eventWhere({ ...e, location: "Main Auditorium" })).toBe(
      "Sat 17 Oct, 4:00 pm · Main Auditorium",
    );
    expect(eventWhere({ ...e, location: null })).toBe("Sat 17 Oct, 4:00 pm");
  });

  it("splits a day into the month and date for the tile", () => {
    expect(dateTile("2026-10-03T00:00:00")).toEqual({
      month: "Oct",
      date: "3",
    });
  });

  it("says how full an event is, never past full", () => {
    expect(capacityShare(75, 150)).toBe(0.5);
    expect(capacityShare(160, 150)).toBe(1);
    expect(capacityShare(12, null)).toBeNull();
    expect(capacityShare(12, 0)).toBeNull();
  });

  it("names the capacity, or says there is none", () => {
    expect(capacityLabel(1200)).toBe("of 1,200");
    expect(capacityLabel(undefined)).toBe("No limit");
  });
});
