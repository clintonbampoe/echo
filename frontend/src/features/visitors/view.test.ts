import { memberIdOf, visitorKind } from "./view";

describe("visitorKind", () => {
  it("is New for the first visit and Returning after that", () => {
    expect(visitorKind({ visitCount: 0 })).toBe("New");
    expect(visitorKind({ visitCount: 1 })).toBe("New");
    expect(visitorKind({ visitCount: 2 })).toBe("Returning");
  });
});

describe("memberIdOf", () => {
  it("is nothing for a visitor who has not joined", () => {
    expect(memberIdOf({ id: "v1" })).toBeNull();
  });

  it("is the visitor's own id once they have a converted date, even without the server's link", () => {
    expect(memberIdOf({ id: "v1", convertedAt: "2026-10-12T09:00:00Z" })).toBe(
      "v1",
    );
  });

  it("prefers the server's link when it is there", () => {
    expect(
      memberIdOf({
        id: "v1",
        convertedAt: "2026-10-12T09:00:00Z",
        convertedToMemberPersonId: "m9",
      }),
    ).toBe("m9");
  });
});
