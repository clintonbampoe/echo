import type { Project } from "./api";
import {
  periodDates,
  toApiFilters,
  projectFilters,
  countActive,
} from "./filters";
import {
  managerOptions,
  percentRaised,
  projectDates,
  projectStats,
} from "./view";

const project = (over: Partial<Project> = {}): Project => ({
  id: "p1",
  name: "Church roof renovation",
  categoryId: 1,
  categoryName: "Building",
  managerId: "m1",
  managerName: "Ama Boateng",
  status: "AtRisk",
  targetAmount: 80000,
  raisedAmount: 48200,
  startDate: "2026-03-01",
  endDate: "2026-12-31",
  description: null,
  createdAt: "2026-01-01T00:00:00Z",
  ...over,
});

describe("project view", () => {
  it("works out the percent raised, even past the target", () => {
    expect(percentRaised(project())).toBe(60);
    expect(percentRaised(project({ raisedAmount: 0 }))).toBe(0);
    expect(percentRaised(project({ raisedAmount: 100000 }))).toBe(125);
    // Rounded down: 99.9% is not yet the target.
    expect(percentRaised(project({ raisedAmount: 79990 }))).toBe(99);
  });

  it("writes the dates, with or without an end", () => {
    expect(projectDates(project())).toBe("Mar 2026 to Dec 2026");
    expect(projectDates(project({ endDate: null }))).toBe("Mar 2026 onward");
  });

  it("shows dashes until the numbers arrive, then the numbers", () => {
    expect(projectStats(undefined, "This year").map((s) => s.value)).toEqual([
      "–",
      "–",
      "–",
      "–",
    ]);
    const stats = projectStats(
      {
        totalProjects: 6,
        totalRaised: 107000,
        totalTarget: 215000,
        atRiskCount: 1,
      },
      "This year",
    );
    expect(stats.map((s) => s.value)).toEqual([
      "6",
      "GH₵ 107,000",
      "GH₵ 215,000",
      "1",
    ]);
    expect(stats[0]?.note).toBe("This year");
  });

  it("lists each manager once, by name", () => {
    const options = managerOptions([
      project(),
      project({ id: "p2", managerId: "m2", managerName: "Kofi Mensah" }),
      project({ id: "p3" }),
    ]);
    expect(options.map((o) => o.label)).toEqual([
      "Manager",
      "Ama Boateng",
      "Kofi Mensah",
    ]);
  });
});

describe("project filters", () => {
  it("covers the whole calendar year, so a project that starts later this year is in it", () => {
    expect(periodDates("year", "2026-10-19")).toEqual({
      from: "2026-01-01",
      to: "2026-12-31",
    });
    expect(periodDates("last-year", "2026-10-19")).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    expect(periodDates("all", "2026-10-19").from).toBe("2000-01-01");
  });

  it("turns the address into API filters and counts what narrows the list", () => {
    const f = {
      ...projectFilters.empty,
      status: "AtRisk",
      category: "3",
      manager: "m1",
      name: "roof",
      window: "all",
    };
    expect(toApiFilters(f, "2026-10-19")).toMatchObject({
      name: "roof",
      status: "AtRisk",
      categoryId: 3,
      managerId: "m1",
      from: "2000-01-01",
    });
    expect(countActive(f)).toBe(4);
    expect(countActive(projectFilters.empty)).toBe(0);
  });
});
