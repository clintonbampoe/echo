import { initials } from "./initials";

describe("initials", () => {
  it("takes the first letter of the first two words", () => {
    expect(initials("Grace Chapel")).toBe("GC");
    expect(initials("Grace Chapel International")).toBe("GC");
    expect(initials("  ama  ")).toBe("A");
    expect(initials("")).toBe("");
  });
});
