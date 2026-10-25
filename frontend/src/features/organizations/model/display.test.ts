import { memberCountLabel, roles, toMemberFilters } from "./display";

describe("organization view", () => {
  it("counts members, in the singular for one", () => {
    expect(memberCountLabel(61)).toBe("61 members");
    expect(memberCountLabel(1)).toBe("1 member");
    expect(memberCountLabel(0)).toBe("0 members");
    expect(memberCountLabel(1204)).toBe("1.2k members");
  });

  it("turns the role filter into what the API takes, ignoring anything that is not a role", () => {
    expect(toMemberFilters("Leader")).toEqual({
      role: "Leader",
      name: undefined,
    });
    expect(toMemberFilters("")).toEqual({ role: undefined, name: undefined });
    expect(toMemberFilters("Pastor")).toEqual({
      role: undefined,
      name: undefined,
    });
    expect(toMemberFilters("", "  ko ")).toEqual({
      role: undefined,
      name: "ko",
    });
    expect(toMemberFilters("", "   ")).toEqual({
      role: undefined,
      name: undefined,
    });
    expect(roles).toEqual(["Leader", "Secretary", "Member"]);
  });
});
