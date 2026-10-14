import { memberFilters, toApiFilters } from "./filters";

const { empty } = memberFilters;

describe("member filters", () => {
  it("reads and writes the address through the shared filters", () => {
    const filters = memberFilters.read(
      new URLSearchParams("status=Active&window=all&junk=1"),
    );
    expect(filters).toEqual({ ...empty, status: "Active", window: "all" });
    expect(memberFilters.write(filters).toString()).toBe(
      "status=Active&window=all",
    );
  });

  it("falls back to this year for an unknown window", () => {
    expect(
      memberFilters.read(new URLSearchParams("window=whenever")).window,
    ).toBe("year");
  });

  it("sends no dates for this year, and whole-year dates for last year", () => {
    const now = new Date("2026-10-14T12:00:00Z");
    expect(toApiFilters(empty, now)).toEqual({});
    expect(
      toApiFilters({ ...empty, name: " ama ", window: "last-year" }, now),
    ).toEqual({
      name: "ama",
      from: "2025-01-01",
      to: "2025-12-31",
    });
  });
});
