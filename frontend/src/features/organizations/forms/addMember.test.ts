import { addMemberSchema, emptyAddMember, toMemberInput } from "./addMember";

describe("add member form", () => {
  it("starts as an ordinary member who joined today", () => {
    expect(emptyAddMember("2026-10-15")).toEqual({
      memberId: "",
      role: "Member",
      joinedAt: "2026-10-15",
    });
  });

  it("asks for a member before anything else", () => {
    const result = addMemberSchema.safeParse(emptyAddMember("2026-10-15"));
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.message).toBe("Choose a member");
  });

  it("asks for the day they joined", () => {
    const result = addMemberSchema.safeParse({
      ...emptyAddMember(""),
      memberId: "m1",
    });
    expect(result.error?.issues[0]?.message).toBe("Enter the day they joined");
  });

  it("turns the form into what the API wants", () => {
    expect(
      toMemberInput("o1", {
        memberId: "m1",
        role: "Secretary",
        joinedAt: "2026-10-15",
      }),
    ).toEqual({
      memberId: "m1",
      organizationId: "o1",
      role: "Secretary",
      joinedAt: "2026-10-15",
    });
  });
});
