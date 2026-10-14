import { organizationsStat } from "./stat";

describe("organizationsStat", () => {
  it("is the count and up to three names", () => {
    expect(organizationsStat(["Choir", "Ushers"], false)).toMatchObject({
      value: "2",
      note: "Choir, Ushers",
    });
    expect(organizationsStat(["A", "B", "C", "D", "E"], false).note).toBe(
      "A, B, C +2",
    );
  });
  it("is a dash while loading", () => {
    expect(organizationsStat([], true).value).toBe("–");
  });
});
