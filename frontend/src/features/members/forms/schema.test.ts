import { emptyMember, toInput } from "./mapping";
import { memberSchema, type MemberValues } from "./schema";

const valid: MemberValues = {
  ...emptyMember,
  firstName: "Kwame",
  lastName: "Asante",
  phoneNumber: "024 412 7788",
  dateOfBirth: "1988-02-14",
};

const messages = (v: MemberValues) => {
  const result = memberSchema.safeParse(v);
  return result.success ? [] : result.error.issues.map((i) => i.message);
};

describe("member form rules", () => {
  it("accepts a member with only the required fields filled in", () => {
    expect(messages(valid)).toEqual([]);
  });

  it("says what is missing in plain words", () => {
    expect(messages(emptyMember)).toEqual(
      expect.arrayContaining([
        "Enter a first name",
        "Enter a phone number",
        "Enter a date of birth",
      ]),
    );
  });

  it("checks an optional contact phone only when it is filled in", () => {
    expect(
      messages({ ...valid, emergencyContactPhoneNumber: "call me" }),
    ).toEqual(["Enter a valid phone number"]);
    expect(
      messages({ ...valid, emergencyContactPhoneNumber: "055 301 4408" }),
    ).toEqual([]);
  });

  it("rejects a bad email, a bad phone number and a birth date in the future", () => {
    expect(messages({ ...valid, emailAddress: "ama.boateng@" })).toEqual([
      "Enter a valid email address",
    ]);
    expect(messages({ ...valid, phoneNumber: "call me" })).toEqual([
      "Enter a valid phone number",
    ]);
    expect(messages({ ...valid, dateOfBirth: "2999-01-01" })).toEqual([
      "Birth date cannot be in the future",
    ]);
  });

  it("leaves empty optional fields out of what is sent", () => {
    const sent = toInput(valid);
    expect(sent.emailAddress).toBeUndefined();
    expect(sent.joinedDate).toBeUndefined();
    expect(sent.firstName).toBe("Kwame");
    expect(sent.region).toBeUndefined();
    expect(sent.maritalStatus).toBeUndefined();
    expect(sent.city).toBeUndefined();
  });
});
