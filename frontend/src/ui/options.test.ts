import { filterOptions, plainOptions, withAny } from "./options";

describe("options", () => {
  it("uses each value as its own label", () => {
    expect(plainOptions(["Male", "Female"])).toEqual([
      { value: "Male", label: "Male" },
      { value: "Female", label: "Female" },
    ]);
  });

  it('puts an "Any" choice with an empty value first', () => {
    expect(withAny("Any status", plainOptions(["Active"]))).toEqual([
      { value: "", label: "Any status" },
      { value: "Active", label: "Active" },
    ]);
  });

  it("filters by a part of the label, ignoring case and outer spaces", () => {
    const list = plainOptions([
      "Ashanti",
      "Western North",
      "Savannah",
      "Northern",
    ]);
    expect(filterOptions(list, " ASH ").map((o) => o.value)).toEqual([
      "Ashanti",
    ]);
    expect(filterOptions(list, "nor").map((o) => o.value)).toEqual([
      "Western North",
      "Northern",
    ]);
    expect(filterOptions(list, "")).toBe(list);
    expect(filterOptions(list, "zzz")).toEqual([]);
  });
});
