import { required } from "./validation";

describe("required", () => {
  it("rejects empty and blank text with the given message", () => {
    expect(
      required("Enter a name").safeParse("   ").error?.issues[0]?.message,
    ).toBe("Enter a name");
    expect(required("Enter a name").safeParse("Ama").success).toBe(true);
  });
});
