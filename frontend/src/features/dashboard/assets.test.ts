import { amountText, assetHint, assetView, rowsBy } from "./assets";

const figures = { count: 42, value: 110000, cost: 148000, lost: 38000 };

describe("assetView", () => {
  it("says what the assets are worth against what they cost", () => {
    expect(assetView(figures)).toEqual({
      value: "GH₵ 110,000",
      share: 110000 / 148000,
      note: "74% of what they cost",
      cost: "GH₵ 148,000",
      lost: "GH₵ 38,000",
    });
  });

  it("copes with no cost recorded, and with nothing yet", () => {
    expect(assetView({ ...figures, cost: 0 }).note).toBe(
      "No purchase cost recorded",
    );
    expect(assetView(undefined).value).toBeUndefined();
  });
});

describe("the rest", () => {
  it("counts the assets in the line under the title", () => {
    expect(assetHint(42)).toBe("42 assets, not counting liquidated");
    expect(assetHint(1)).toBe("1 asset, not counting liquidated");
    expect(assetHint()).toBe("Not counting liquidated");
  });

  it("measures each category by value or by count", () => {
    const rows = [{ key: "1", name: "Vehicles", count: 2, value: 46000 }];
    expect(rowsBy(rows, "value")[0]?.total).toBe(46000);
    expect(rowsBy(rows, "count")[0]?.total).toBe(2);
    expect(amountText(46000, "value")).toBe("GH₵ 46,000");
    expect(amountText(2, "count")).toBe("2");
  });
});
