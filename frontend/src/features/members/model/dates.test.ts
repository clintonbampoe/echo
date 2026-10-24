import { memberFor } from "./dates";

describe("memberFor", () => {
  it("says years, months, or new", () => {
    expect(memberFor("2019-03-12", "2026-10-14")).toBe("Member for 7 years");
    expect(memberFor("2025-10-14", "2026-10-13")).toBe("Member for 11 months");
    expect(memberFor("2026-10-01", "2026-10-14")).toBe("New member");
    expect(memberFor("2025-09-14", "2026-10-14")).toBe("Member for 1 year");
  });
  it("is empty without a joined date", () => {
    expect(memberFor(null, "2026-10-14")).toBe("");
  });
});
