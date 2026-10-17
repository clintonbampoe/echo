import { fromClock, spoken, timeNow, toClock } from "./time";

describe("time of day", () => {
  it("reads 24 hour text as a 12 hour clock", () => {
    expect(toClock("00:05")).toEqual({ hour: 12, minute: 5, pm: false });
    expect(toClock("12:00")).toEqual({ hour: 12, minute: 0, pm: true });
    expect(toClock("14:30")).toEqual({ hour: 2, minute: 30, pm: true });
    expect(toClock("08:00")).toEqual({ hour: 8, minute: 0, pm: false });
  });

  it("writes the clock back as 24 hour text", () => {
    for (const time of ["00:05", "08:00", "11:59", "12:00", "12:30", "23:59"])
      expect(fromClock(toClock(time))).toBe(time);
  });

  it("says a time the way the app shows it", () => {
    expect(spoken("00:05")).toBe("12:05 am");
    expect(spoken("18:40")).toBe("6:40 pm");
  });

  it("writes the time now with a leading zero", () => {
    expect(timeNow(new Date(2026, 9, 17, 7, 4))).toBe("07:04");
  });
});
