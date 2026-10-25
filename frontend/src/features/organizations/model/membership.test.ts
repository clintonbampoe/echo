import {
  fromMembership,
  membershipSchema,
  toMembershipInput,
} from "./membership";

describe("membership form", () => {
  it("starts with the role and the day they joined, as a plain date", () => {
    expect(
      fromMembership({ role: "Secretary", joinedAt: "2018-03-11T00:00:00Z" }),
    ).toEqual({ role: "Secretary", joinedAt: "2018-03-11" });
  });

  it("asks for the day they joined", () => {
    const result = membershipSchema.safeParse({ role: "Member", joinedAt: "" });
    expect(result.error?.issues[0]?.message).toBe("Enter the day they joined");
  });

  it("turns the form into what the API wants", () => {
    expect(
      toMembershipInput({ role: "Leader", joinedAt: "2018-03-11" }),
    ).toEqual({ role: "Leader", joinedAt: "2018-03-11" });
  });
});
