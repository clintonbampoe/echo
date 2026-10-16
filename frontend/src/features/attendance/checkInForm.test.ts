import { checkInSchema, timeOf, toCheckedInAt } from "./checkInForm";

describe("the check-in time", () => {
  it("reads the time of day", () => {
    expect(timeOf("2026-10-11T09:04:00")).toBe("09:04");
  });

  it("comes back as the same time on the same day", () => {
    expect(timeOf(toCheckedInAt("2026-10-11", "09:04"))).toBe("09:04");
    expect(new Date(toCheckedInAt("2026-10-11", "09:04")).getDate()).toBe(11);
  });

  it("needs a time", () => {
    expect(checkInSchema.safeParse({ time: "" }).success).toBe(false);
    expect(checkInSchema.safeParse({ time: "09:04" }).success).toBe(true);
  });
});
