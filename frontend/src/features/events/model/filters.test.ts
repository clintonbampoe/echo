import {
  countActive,
  eventFilters,
  toApiFilters,
  windowRange,
} from "./filters";

const now = new Date("2026-10-17T10:00:00");
const today = "2026-10-17";

describe("event filters", () => {
  it("opens on this year, with nothing narrowing the list", () => {
    const f = eventFilters.read(new URLSearchParams());
    expect(f).toEqual({ name: "", organization: "", window: "year" });
    expect(eventFilters.isFiltered(f)).toBe(false);
    expect(countActive(f)).toBe(0);
  });

  it("counts an organization and a changed range, not the search", () => {
    expect(countActive({ name: "x", organization: "o1", window: "all" })).toBe(
      2,
    );
    expect(countActive({ name: "x", organization: "", window: "year" })).toBe(
      0,
    );
  });

  it("spells out the time ranges", () => {
    expect(windowRange("year", now)).toEqual({
      from: "2026-01-01",
      to: "2026-12-31",
    });
    expect(windowRange("last-year", now)).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    expect(windowRange("all", now).from < "2001-01-01").toBe(true);
  });

  it("asks for the rest of this year, soonest first, on the upcoming tab", () => {
    const q = toApiFilters(
      { name: " choir ", organization: "o1", window: "year" },
      "upcoming",
      today,
      now,
    );
    expect(q).toEqual({
      name: "choir",
      organizationId: "o1",
      from: today,
      to: "2026-12-31",
      order: "Asc",
    });
  });

  it("asks for this year up to yesterday, newest first, on the past tab", () => {
    const q = toApiFilters(eventFilters.empty, "past", today, now);
    expect(q).toEqual({
      name: undefined,
      organizationId: undefined,
      from: "2026-01-01",
      to: "2026-10-16",
      order: "Desc",
    });
  });

  it("keeps the whole of last year for the past tab, and leaves nothing for the upcoming one", () => {
    const f = { ...eventFilters.empty, window: "last-year" };
    expect(toApiFilters(f, "past", today, now)).toMatchObject({
      from: "2025-01-01",
      to: "2025-12-31",
    });
    const upcoming = toApiFilters(f, "upcoming", today, now);
    expect(upcoming.from > upcoming.to).toBe(true);
  });
});
