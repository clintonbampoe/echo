import { toApiFilters, toSummaryFilters, visitorFilters } from "./filters";

const { empty } = visitorFilters;
const now = new Date("2026-10-14T12:00:00Z");

describe("visitor filters", () => {
  it("reads and writes the address through the shared filters", () => {
    const filters = visitorFilters.read(
      new URLSearchParams("status=became&window=all&junk=1"),
    );
    expect(filters).toEqual({ ...empty, status: "became", window: "all" });
    expect(visitorFilters.write(filters).toString()).toBe(
      "status=became&window=all",
    );
  });

  it("falls back to the defaults for values it does not know", () => {
    expect(
      visitorFilters.read(
        new URLSearchParams("status=everyone&window=whenever"),
      ),
    ).toEqual(empty);
  });

  it("asks for visitors who are not yet members, this year, by default", () => {
    expect(toApiFilters(empty, now)).toEqual({});
  });

  it('asks for converted visitors only when "Became members" is chosen', () => {
    expect(
      toApiFilters(
        { ...empty, name: " esi ", status: "became", window: "last-year" },
        now,
      ),
    ).toEqual({
      name: "esi",
      converted: true,
      from: "2025-01-01",
      to: "2025-12-31",
    });
  });

  it("gives the summary only the time range", () => {
    expect(
      toSummaryFilters(
        { name: "esi", status: "became", window: "last-year" },
        now,
      ),
    ).toEqual({ from: "2025-01-01", to: "2025-12-31" });
  });
});
