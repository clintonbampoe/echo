import { amountProblem, required } from "./validation";

describe("required", () => {
  it("rejects empty and blank text with the given message", () => {
    expect(
      required("Enter a name").safeParse("   ").error?.issues[0]?.message,
    ).toBe("Enter a name");
    expect(required("Enter a name").safeParse("Ama").success).toBe(true);
  });
});

describe("amountProblem", () => {
  const rule = {
    min: 0,
    max: 1_000_000,
    empty: "Enter it",
    range: "Enter a number between 0 and 1,000,000",
  };

  it("asks for an amount, and for one in range", () => {
    expect(amountProblem("", rule)).toBe("Enter it");
    expect(amountProblem("abc", rule)).toBe(rule.range);
    expect(amountProblem("1.234", rule)).toBe(rule.range);
    expect(amountProblem("1,000,000.01", rule)).toBe(rule.range);
    expect(amountProblem("-5", rule)).toBe(rule.range);
  });

  it("accepts commas, pesewas and the ends of the range", () => {
    expect(amountProblem("0", rule)).toBeNull();
    expect(amountProblem("1,250.50", rule)).toBeNull();
    expect(amountProblem("1,000,000", rule)).toBeNull();
    expect(amountProblem("0", { ...rule, min: 0.01 })).toBe(rule.range);
  });
});
