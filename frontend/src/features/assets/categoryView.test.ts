import type { AssetCategory } from "./api";
import { categorySchema, mergeTargets, usedBy } from "./categoryView";

const equipment: AssetCategory = { id: 1, name: "Equipment", assetCount: 3 };
const property: AssetCategory = { id: 2, name: "Property", assetCount: 0 };

describe("category view", () => {
  it("counts assets in words", () => {
    expect(usedBy(0)).toBe("0 assets");
    expect(usedBy(1)).toBe("1 asset");
    expect(usedBy(3)).toBe("3 assets");
  });

  it("offers every other category to move assets to, never the one being removed", () => {
    expect(mergeTargets([equipment, property], equipment)).toEqual([
      { value: "2", label: "Property" },
    ]);
    expect(mergeTargets([equipment], equipment)).toEqual([]);
  });

  it("needs a name of up to 100 characters", () => {
    expect(categorySchema.safeParse({ name: "  " }).success).toBe(false);
    expect(categorySchema.safeParse({ name: "x".repeat(101) }).success).toBe(
      false,
    );
    expect(categorySchema.safeParse({ name: "Vehicles" }).success).toBe(true);
  });
});
