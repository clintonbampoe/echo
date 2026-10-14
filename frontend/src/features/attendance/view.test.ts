import { filtersFor, isNarrowed, stripSlots } from "./view";

describe("filtersFor", () => {
  it("asks for nothing extra on all time with every type", () => {
    expect(filtersFor("", "all", "2026-10-14")).toEqual({
      sessionLabelId: undefined,
    });
  });
  it("turns the choices into the API filters", () => {
    expect(filtersFor("2", "year", "2026-10-14")).toEqual({
      sessionLabelId: "2",
      from: "2026-01-01",
      to: "2026-10-14",
    });
    expect(filtersFor("", "3m", "2026-10-14")).toEqual({
      sessionLabelId: undefined,
      from: "2026-07-14",
      to: "2026-10-14",
    });
  });
  it("knows when the list is narrowed", () => {
    expect(isNarrowed("", "all")).toBe(false);
    expect(isNarrowed("2", "all")).toBe(true);
    expect(isNarrowed("", "3m")).toBe(true);
  });
});

describe("stripSlots", () => {
  it("puts the oldest session first and marks the ones they attended", () => {
    const sessions = [
      {
        id: "s3",
        date: "2026-10-11",
        name: null,
        sessionLabelName: "Sunday Service",
      },
      {
        id: "s2",
        date: "2026-10-04",
        name: "Harvest Sunday",
        sessionLabelName: "Sunday Service",
      },
      {
        id: "s1",
        date: "2026-09-27",
        name: null,
        sessionLabelName: "Sunday Service",
      },
    ];
    expect(stripSlots(sessions, new Set(["s3", "s1"]))).toEqual([
      { id: "s1", date: "2026-09-27", name: "Sunday Service", present: true },
      { id: "s2", date: "2026-10-04", name: "Harvest Sunday", present: false },
      { id: "s3", date: "2026-10-11", name: "Sunday Service", present: true },
    ]);
  });
});
