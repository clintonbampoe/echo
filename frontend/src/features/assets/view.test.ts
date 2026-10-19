import type { Asset } from "./api";
import {
  assetFilters,
  countActive,
  periodDates,
  toApiFilters,
} from "./filters";
import { assetFigures, assetStats, purchased, valueChange } from "./view";

const asset = (over: Partial<Asset> = {}): Asset => ({
  id: "a1",
  name: "Behringer X32 mixing desk",
  categoryId: 1,
  categoryName: "Equipment",
  serialNumber: "BX32-88214",
  purchaseDate: "2023-03-12",
  purchaseCost: 42000,
  currentValue: 31500,
  status: "InUse",
  description: null,
  createdAt: "2026-01-01T00:00:00Z",
  ...over,
});

describe("asset view", () => {
  it("works out what an asset has lost, or gained, against its cost", () => {
    expect(valueChange(42000, 31500)).toEqual({
      label: "Lost value",
      amount: 10500,
      note: "25% below cost",
    });
    expect(valueChange(1000, 1000)).toEqual({
      label: "Lost value",
      amount: 0,
      note: "Same as cost",
    });
    expect(valueChange(100000, 150000)).toEqual({
      label: "Gained value",
      amount: 50000,
      note: "50% above cost",
    });
    expect(valueChange(0, 500)).toEqual({
      label: "Gained value",
      amount: 500,
      note: "Cost not recorded",
    });
  });

  it("writes the day of purchase, or a dash", () => {
    expect(purchased(asset())).toBe("12 Mar 2023");
    expect(purchased(asset({ purchaseDate: null }))).toBe("–");
  });

  it("shows dashes until the numbers arrive, then the numbers", () => {
    expect(assetStats(undefined, "All time", true).map((s) => s.value)).toEqual(
      ["–", "–", "–", "–"],
    );
    const stats = assetStats(
      {
        totalAssets: 5,
        totalCurrentValue: 177900,
        totalPurchaseCost: 325200,
        totalDepreciation: 147300,
      },
      "All time",
      true,
    );
    expect(stats.map((s) => s.value)).toEqual([
      "5",
      "GH₵ 177,900",
      "GH₵ 325,200",
      "GH₵ 147,300",
    ]);
    expect(stats[0]?.note).toBe("All time");
    expect(stats[1]?.note).toBe("Not counting liquidated");
  });

  it("does not say liquidated assets are left out when the list is of them", () => {
    const stats = assetStats(
      {
        totalAssets: 1,
        totalCurrentValue: 0,
        totalPurchaseCost: 9000,
        totalDepreciation: 9000,
      },
      "All time",
      false,
    );
    expect(stats[1]?.note).toBe("Across these assets");
  });

  it("calls a total that is worth more than it cost a gain", () => {
    const stats = assetStats(
      {
        totalAssets: 1,
        totalCurrentValue: 150000,
        totalPurchaseCost: 100000,
        totalDepreciation: -50000,
      },
      "All time",
      true,
    );
    expect(stats[3]).toMatchObject({
      label: "Gained value",
      value: "GH₵ 50,000",
      note: "Current value minus cost",
    });
  });

  it("gives the three numbers of one asset", () => {
    expect(assetFigures(asset())).toEqual([
      { label: "Current value", value: "GH₵ 31,500", note: "As last entered" },
      { label: "Purchase cost", value: "GH₵ 42,000", note: "12 Mar 2023" },
      { label: "Lost value", value: "GH₵ 10,500", note: "25% below cost" },
    ]);
    expect(assetFigures(asset({ purchaseDate: null }))[1]?.note).toBe(
      "No purchase date",
    );
  });
});

describe("asset filters", () => {
  it("covers all time by default, and a whole calendar year when asked", () => {
    expect(periodDates("all", "2026-10-19")).toEqual({
      from: "2000-01-01",
      to: "2100-12-31",
    });
    expect(periodDates("year", "2026-10-19")).toEqual({
      from: "2026-01-01",
      to: "2026-12-31",
    });
    expect(periodDates("last-year", "2026-10-19")).toEqual({
      from: "2025-01-01",
      to: "2025-12-31",
    });
  });

  it("counts what narrows the list, not the search or the default period", () => {
    expect(countActive(assetFilters.empty)).toBe(0);
    expect(countActive({ ...assetFilters.empty, name: "desk" })).toBe(0);
    expect(
      countActive({ name: "", status: "InUse", category: "2", window: "year" }),
    ).toBe(3);
  });

  it("turns the address into what the server is asked", () => {
    expect(
      toApiFilters(
        { name: "desk", status: "InStorage", category: "3", window: "year" },
        "2026-10-19",
      ),
    ).toEqual({
      name: "desk",
      status: "InStorage",
      categoryId: 3,
      from: "2026-01-01",
      to: "2026-12-31",
    });
    expect(toApiFilters(assetFilters.empty, "2026-10-19")).toMatchObject({
      name: undefined,
      status: undefined,
      categoryId: undefined,
      from: "2000-01-01",
    });
  });
});
