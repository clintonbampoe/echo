import {
  chooseForMonth,
  collectedDates,
  countActive,
  forMonthLabel,
  parseForMonth,
  titheFilters,
  toApiFilters,
} from "./filters";

const today = "2026-10-14";

describe("tithe filters", () => {
  it("reads a month, a whole year, or nothing from the address text", () => {
    expect(parseForMonth("2026-10")).toEqual({ year: 2026, month: 10 });
    expect(parseForMonth("2026")).toEqual({ year: 2026, month: undefined });
    expect(parseForMonth("2026-13")).toBeNull();
    expect(parseForMonth("oct")).toBeNull();
    expect(forMonthLabel("2026-10")).toBe("October 2026");
    expect(forMonthLabel("2026")).toBe("2026");
    expect(forMonthLabel("")).toBe("");
  });

  it("turns every collected range into plain days", () => {
    expect(collectedDates("week", today)).toEqual({
      from: "2026-10-11",
      to: today,
    });
    expect(collectedDates("month", today)).toEqual({
      from: "2026-10-01",
      to: today,
    });
    expect(collectedDates("year", today)).toEqual({
      from: "2026-01-01",
      to: today,
    });
    expect(collectedDates("last-year", today)).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    expect(collectedDates("all", today).from < "2001-01-01").toBe(true);
  });

  it("builds the API filters from the page filters", () => {
    const f = {
      ...titheFilters.empty,
      name: " ko ",
      forMonth: "2026-09",
      method: "Cash",
    };
    expect(toApiFilters(f, today)).toEqual({
      memberName: "ko",
      year: 2026,
      month: "September",
      paymentMethod: "Cash",
      from: "2026-10-11",
      to: today,
    });
    expect(toApiFilters(titheFilters.empty, today)).toMatchObject({
      memberName: undefined,
      year: undefined,
      month: undefined,
      paymentMethod: undefined,
    });
  });

  it("counts what narrows the list, not the search box, and not the default week", () => {
    expect(countActive(titheFilters.empty)).toBe(0);
    expect(
      countActive({ ...titheFilters.empty, name: "ko", window: "week" }),
    ).toBe(0);
    expect(
      countActive({
        name: "",
        forMonth: "2026-10",
        method: "Cash",
        window: "year",
      }),
    ).toBe(3);
  });

  it("widens the default week when a month is chosen, and leaves a chosen range alone", () => {
    expect(chooseForMonth(titheFilters.empty, "2026-09")).toEqual({
      forMonth: "2026-09",
      window: "all",
    });
    expect(
      chooseForMonth({ ...titheFilters.empty, window: "year" }, "2026-09"),
    ).toEqual({ forMonth: "2026-09", window: "year" });
    expect(
      chooseForMonth({ ...titheFilters.empty, window: "all" }, ""),
    ).toEqual({ forMonth: "", window: "all" });
    expect(chooseForMonth(titheFilters.empty, "")).toEqual({
      forMonth: "",
      window: "week",
    });
  });

  it("treats anything but the defaults as filtered", () => {
    expect(titheFilters.isFiltered(titheFilters.empty)).toBe(false);
    expect(
      titheFilters.isFiltered({ ...titheFilters.empty, window: "month" }),
    ).toBe(true);
  });
});
