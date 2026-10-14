import { urlFilters } from "./urlFilters";

const filters = urlFilters(
  { name: "", status: "", window: "year" },
  { window: ["year", "last-year", "all"] },
);

describe("urlFilters", () => {
  it("reads from the address and writes back only what differs from the defaults", () => {
    const values = filters.read(
      new URLSearchParams("status=Active&window=all&junk=1"),
    );
    expect(values).toEqual({ name: "", status: "Active", window: "all" });
    expect(filters.write(values).toString()).toBe("status=Active&window=all");
    expect(filters.write(filters.empty).toString()).toBe("");
  });

  it("falls back to the default for a value that is not allowed", () => {
    expect(filters.read(new URLSearchParams("window=whenever")).window).toBe(
      "year",
    );
  });

  it("knows when anything is narrowing the list", () => {
    expect(filters.isFiltered(filters.empty)).toBe(false);
    expect(filters.isFiltered({ ...filters.empty, window: "all" })).toBe(true);
    expect(filters.isFiltered({ ...filters.empty, name: "ama" })).toBe(true);
  });
});
