import { plainOptions, withAny } from "./options";

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
});
