import { plainOptions } from "@/lib/options";
import { filterOptions } from "./options";

describe("options", () => {
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
