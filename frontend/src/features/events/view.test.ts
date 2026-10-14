import { eventStatus } from "./view";

describe("eventStatus", () => {
  it("is attended when they were checked in", () => {
    expect(eventStatus("2026-03-10", true, "2026-10-14")).toBe("attended");
  });
  it("is upcoming on the day itself and after", () => {
    expect(eventStatus("2026-10-14", false, "2026-10-14")).toBe("upcoming");
    expect(eventStatus("2026-10-18", false, "2026-10-14")).toBe("upcoming");
  });
  it("is missed once the day has passed without a check-in", () => {
    expect(eventStatus("2026-10-13", false, "2026-10-14")).toBe("missed");
  });
});
