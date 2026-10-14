import { countLabel } from "./paged";

describe("countLabel", () => {
  it("is the number when everything is loaded", () => {
    expect(countLabel(9, false)).toBe("9");
  });
  it('says "at least" when there is more to load', () => {
    expect(countLabel(50, true)).toBe("50+");
  });
});
