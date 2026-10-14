import { windowDates } from "./windows";

describe("windowDates", () => {
  const now = new Date("2026-10-14T12:00:00Z");
  it("sends no dates for this year", () => {
    expect(windowDates("year", now)).toEqual({});
  });
  it("sends whole-year dates for last year", () => {
    expect(windowDates("last-year", now)).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
  });
  it("sends a very wide range for all time", () => {
    expect(windowDates("all", now)).toEqual({
      from: "1900-01-01",
      to: "2999-12-31",
    });
  });
});
