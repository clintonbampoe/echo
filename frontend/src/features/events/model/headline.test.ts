import { eventDates, eventHeadline } from "./headline";

describe("event headline", () => {
  const day = {
    startDate: "2026-10-03",
    endDate: "2026-10-03",
    startTime: "06:00:00",
    endTime: "18:00:00",
  };

  it("gives a one-day event as the day, then from when to when", () => {
    expect(eventDates(day)).toBe("Sat 3 Oct 2026, 6:00 am to 6:00 pm");
  });

  it("leaves out what is not set", () => {
    expect(eventDates({ ...day, endTime: null })).toBe(
      "Sat 3 Oct 2026, 6:00 am",
    );
    expect(eventDates({ ...day, startTime: null, endTime: null })).toBe(
      "Sat 3 Oct 2026",
    );
  });

  it("gives both ends of an event over several days", () => {
    expect(
      eventDates({
        startDate: "2026-10-30",
        endDate: "2026-11-01",
        startTime: "08:00:00",
        endTime: null,
      }),
    ).toBe("Fri 30 Oct 2026, 8:00 am to Sun 1 Nov 2026");
  });

  it("adds the place and who is in charge", () => {
    expect(
      eventHeadline({
        ...day,
        location: "Aburi Mountain Lodge",
        organizerName: "Efua Quaye",
      }),
    ).toBe(
      "Sat 3 Oct 2026, 6:00 am to 6:00 pm. Aburi Mountain Lodge. Organized by Efua Quaye.",
    );
    expect(
      eventHeadline({ ...day, location: null, organizerName: "Efua Quaye" }),
    ).toBe("Sat 3 Oct 2026, 6:00 am to 6:00 pm. Organized by Efua Quaye.");
  });
});
