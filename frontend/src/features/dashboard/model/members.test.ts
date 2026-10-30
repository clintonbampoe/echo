import type { MemberMix } from "../api/members";
import { statusRows } from "./members";

const mix: MemberMix = {
  total: 248,
  active: 201,
  male: 114,
  female: 134,
  average: 36,
  bands: [],
  joinedThisYear: 16,
};

describe("statusRows", () => {
  it("gives active, inactive (everyone else) and new this year", () => {
    expect(statusRows(mix, 16)).toEqual([
      { key: "active", name: "Active", count: 201 },
      { key: "inactive", name: "Inactive", count: 47 },
      { key: "new", name: "New this year", count: 16 },
    ]);
  });

  it("leaves out new this year until it is known, and everything before the members are", () => {
    expect(statusRows(mix, undefined).map((r) => r.key)).toEqual([
      "active",
      "inactive",
    ]);
    expect(statusRows(undefined, 3)).toEqual([]);
  });

  it("never counts a negative number as inactive", () => {
    expect(statusRows({ ...mix, active: 300 }, 0)[1]?.count).toBe(0);
  });
});
