import { categoryMark } from "./categoryIcons";

describe("categoryMark", () => {
  it("finds the mark a name mentions, in any case", () => {
    expect(categoryMark("Vehicles").key).toBe("vehicle");
    expect(categoryMark("audio and video").key).toBe("audio");
    expect(categoryMark("SAFETY GEAR").key).toBe("safety");
    expect(categoryMark("Church property").key).toBe("property");
  });

  it("gives a name it does not know the box", () => {
    expect(categoryMark("Equipment").key).toBe("equipment");
    expect(categoryMark("Banners").key).toBe("equipment");
  });
});
