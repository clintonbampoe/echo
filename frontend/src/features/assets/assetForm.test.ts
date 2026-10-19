import type { Asset } from "./api";
import {
  assetSchema,
  emptyAsset,
  fromAsset,
  toAssetInput,
  toAssetUpdate,
} from "./assetForm";

const desk: Asset = {
  id: "a1",
  name: "Behringer X32 mixing desk",
  categoryId: 1,
  categoryName: "Equipment",
  serialNumber: "BX32-88214",
  purchaseDate: "2023-03-12",
  purchaseCost: 42000,
  currentValue: 31500,
  status: "InUse",
  description: "Main desk.",
  createdAt: "2026-01-01T00:00:00Z",
};
const filled = {
  ...emptyAsset,
  name: "Projector",
  categoryId: "1",
  purchaseCost: "6,200",
  currentValue: "5,400",
};
const problems = (values: object, locked = false) => {
  const r = assetSchema(locked).safeParse(values);
  return r.success
    ? []
    : r.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`);
};

describe("asset form", () => {
  it("starts active with everything else to fill in", () => {
    expect(emptyAsset).toEqual({
      name: "",
      categoryId: "",
      status: "Active",
      serialNumber: "",
      purchaseDate: "",
      purchaseCost: "",
      currentValue: "",
      description: "",
    });
  });

  it("asks for a name, a category, a cost and a value, and nothing more", () => {
    expect(problems(emptyAsset)).toEqual([
      "name: Enter an asset name",
      "categoryId: Choose a category",
      "purchaseCost: Enter a cost between 0 and 1,000,000",
      "currentValue: Enter a value between 0 and 1,000,000",
    ]);
    expect(problems(filled)).toEqual([]);
  });

  it("takes a cost or value of nothing, but not above a million or below nothing", () => {
    expect(
      problems({ ...filled, purchaseCost: "0", currentValue: "0" }),
    ).toEqual([]);
    expect(problems({ ...filled, purchaseCost: "1,000,000.01" })).toEqual([
      "purchaseCost: Enter a cost between 0 and 1,000,000",
    ]);
    expect(problems({ ...filled, currentValue: "-1" })).toEqual([
      "currentValue: Enter a value between 0 and 1,000,000",
    ]);
  });

  it("keeps the name, serial number and description within the server limits", () => {
    expect(problems({ ...filled, name: "x".repeat(101) })).toEqual([
      "name: Use 100 characters or fewer",
    ]);
    expect(problems({ ...filled, serialNumber: "x".repeat(101) })).toEqual([
      "serialNumber: Use 100 characters or fewer",
    ]);
    expect(problems({ ...filled, description: "x".repeat(2001) })).toEqual([
      "description: Use 2000 characters or fewer",
    ]);
  });

  it("does not let a purchase date that is set be taken away", () => {
    expect(problems({ ...filled, purchaseDate: "" }, true)).toEqual([
      "purchaseDate: A purchase date can be changed, not removed",
    ]);
    expect(problems({ ...filled, purchaseDate: "" }, false)).toEqual([]);
    expect(problems({ ...filled, purchaseDate: "2024-01-01" }, true)).toEqual(
      [],
    );
  });

  it("turns an asset into the form and back", () => {
    const values = fromAsset(desk);
    expect(values).toEqual({
      name: "Behringer X32 mixing desk",
      categoryId: "1",
      status: "InUse",
      serialNumber: "BX32-88214",
      purchaseDate: "2023-03-12",
      purchaseCost: "42,000.00",
      currentValue: "31,500.00",
      description: "Main desk.",
    });
    expect(toAssetInput(values)).toEqual({
      categoryId: 1,
      name: "Behringer X32 mixing desk",
      status: "InUse",
      serialNumber: "BX32-88214",
      purchaseDate: "2023-03-12",
      purchaseCost: 42000,
      currentValue: 31500,
      description: "Main desk.",
    });
  });

  it("leaves out what was not filled in when creating", () => {
    const input = toAssetInput(filled);
    expect(input).toEqual({
      categoryId: 1,
      name: "Projector",
      status: "Active",
      purchaseCost: 6200,
      currentValue: 5400,
    });
    expect(input).not.toHaveProperty("serialNumber", "");
  });

  it("sends an emptied serial number or description as empty text when saving changes, because leaving it out keeps the old one", () => {
    const values = { ...fromAsset(desk), serialNumber: " ", description: "" };
    expect(toAssetUpdate(values)).toMatchObject({
      serialNumber: "",
      description: "",
      purchaseDate: "2023-03-12",
      purchaseCost: 42000,
    });
  });
});
