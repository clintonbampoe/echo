import { genderRows, joinedLabel, visitorRows } from "./people";

describe("visitorRows", () => {
  it("counts who is still visiting as the ones who have not become members", () => {
    const rows = visitorRows({ added: 4, joined: 1 });
    expect(rows.map((r) => r.value)).toEqual([4, 1, 3]);
    expect(rows.map((r) => r.share)).toEqual([1, 0.25, 0.75]);
  });

  it("never goes below zero, when more became members than were added", () => {
    expect(visitorRows({ added: 1, joined: 3 })[2]?.value).toBe(0);
  });

  it("has no bars for nobody, and no numbers until they arrive", () => {
    expect(visitorRows({ added: 0, joined: 0 }).map((r) => r.share)).toEqual([
      0, 0, 0,
    ]);
    expect(visitorRows(undefined)[0]?.value).toBeUndefined();
  });
});

describe("the rest", () => {
  it("words the line for one or many", () => {
    expect(joinedLabel(1)).toBe("Visitor became a member");
    expect(joinedLabel(3)).toBe("Visitors became members");
    expect(joinedLabel()).toBe("Visitors who became members");
  });

  it("puts everyone who is neither under Other", () => {
    expect(
      genderRows({ total: 1284, male: 692, female: 578 })[2],
    ).toMatchObject({
      name: "Other",
      total: 14,
    });
    expect(genderRows(undefined)).toEqual([]);
  });
});
