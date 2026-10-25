import { contactLine, memberSince, profileLine, pronouns } from "./display";

describe("member display lines", () => {
  it("joins what we know with dots and skips what we do not", () => {
    expect(contactLine({ phoneNumber: "024 412 7788", city: "Accra" })).toBe(
      "024 412 7788 · Accra",
    );
    expect(contactLine({ phoneNumber: "024 412 7788", city: "" })).toBe(
      "024 412 7788",
    );
  });

  it("says when someone joined, if known", () => {
    expect(memberSince("2024-03-12")).toBe("Member since Mar 2024");
    expect(memberSince(null)).toBe("");
    expect(
      profileLine({
        phoneNumber: "024 412 7788",
        city: "Accra",
        joinedDate: "2024-03-12",
      }),
    ).toBe("024 412 7788 · Accra · Member since Mar 2024");
  });
});

describe("pronouns", () => {
  it("follows the gender on record, and falls back to they", () => {
    expect(pronouns("Female")).toEqual({
      subject: "She",
      possessive: "Her",
      leaves: "leaves",
    });
    expect(pronouns("Male").subject).toBe("He");
    expect(pronouns("Other")).toEqual({
      subject: "They",
      possessive: "Their",
      leaves: "leave",
    });
  });
});
