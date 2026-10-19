import {
  firstName,
  formatAgo,
  formatClock,
  formatDate,
  formatDayTime,
  formatMoney,
  formatMonth,
  formatNumber,
  formatTime,
  plural,
} from "./format";

describe("format", () => {
  it("writes a plain date the same way in every browser", () => {
    expect(formatDate("2019-03-12")).toBe("12 Mar 2019");
    expect(formatDate("2022-09-18T00:00:00")).toBe("18 Sep 2022");
  });

  it("groups thousands", () => {
    expect(formatNumber(1284)).toBe("1,284");
  });

  it("writes a time of day the short way", () => {
    expect(formatTime("2026-10-11T09:04:00")).toBe("9:04 am");
    expect(formatTime("2026-10-07T18:12:00")).toBe("6:12 pm");
  });

  it("writes a bare time of day", () => {
    expect(formatClock("08:42:00")).toBe("8:42 am");
    expect(formatClock("22:05:00")).toBe("10:05 pm");
    expect(formatClock("00:15:00")).toBe("12:15 am");
    expect(formatClock("12:00:00")).toBe("12:00 pm");
  });

  it("writes money in cedis", () => {
    expect(formatMoney(6400)).toBe("GH₵ 6,400");
    expect(formatMoney(1250.5)).toBe("GH₵ 1,250.50");
  });

  it("takes the first word of a name", () => {
    expect(firstName("Ama Mensah")).toBe("Ama");
    expect(firstName("  Kofi  ")).toBe("Kofi");
    expect(firstName("")).toBe("");
  });

  it("writes the month someone joined", () => {
    expect(formatMonth("2016-01-05")).toBe("Jan 2016");
    expect(formatMonth("2025-10-31T00:00:00")).toBe("Oct 2025");
  });

  it("writes the day and time of an event", () => {
    expect(formatDayTime("2026-11-14", "17:00:00")).toBe("Sat 14 Nov, 5:00 pm");
    expect(formatDayTime("2026-12-20")).toBe("Sun 20 Dec");
  });

  it("says how long ago in the biggest whole unit", () => {
    const today = "2026-10-14";
    expect(formatAgo("2016-02-03", today)).toBe("10 years ago");
    expect(formatAgo("2025-10-15", today)).toBe("11 months ago");
    expect(formatAgo("2025-10-14", today)).toBe("1 year ago");
    expect(formatAgo("2026-09-14", today)).toBe("1 month ago");
    expect(formatAgo("2026-09-15", today)).toBe("29 days ago");
    expect(formatAgo("2026-10-13", today)).toBe("Yesterday");
    expect(formatAgo("2026-10-14", today)).toBe("Today");
  });
});

describe("plural", () => {
  it("says one of a thing in the singular and any other number in the plural", () => {
    expect(plural(1, "asset")).toBe("1 asset");
    expect(plural(0, "asset")).toBe("0 assets");
    expect(plural(1284, "asset")).toBe("1,284 assets");
    expect(plural(2, "category", "categories")).toBe("2 categories");
  });
});
