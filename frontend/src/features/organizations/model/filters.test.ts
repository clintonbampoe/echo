import { organizationFilters, toApiFilters, toSummaryFilters } from "./filters";

const { empty } = organizationFilters;
const now = new Date("2026-10-14T12:00:00Z");

describe("organization filters", () => {
  it("reads and writes the address through the shared filters", () => {
    const filters = organizationFilters.read(
      new URLSearchParams("name=choir&window=year&junk=1"),
    );
    expect(filters).toEqual({ name: "choir", window: "year" });
    expect(organizationFilters.write(filters).toString()).toBe(
      "name=choir&window=year",
    );
  });

  it("falls back to the defaults for values it does not know", () => {
    expect(
      organizationFilters.read(new URLSearchParams("window=whenever")),
    ).toEqual(empty);
  });

  it("starts at all time, so groups older than this year are not hidden", () => {
    expect(toApiFilters(empty, now)).toEqual({
      from: "1900-01-01",
      to: "2999-12-31",
    });
    expect(organizationFilters.isFiltered(empty)).toBe(false);
  });

  it("sends no dates for this year, which is the server default", () => {
    expect(toApiFilters({ name: " choir ", window: "year" }, now)).toEqual({
      name: "choir",
    });
  });

  it("gives the summary only the time range", () => {
    expect(
      toSummaryFilters({ name: "choir", window: "last-year" }, now),
    ).toEqual({ from: "2025-01-01", to: "2025-12-31" });
  });
});
