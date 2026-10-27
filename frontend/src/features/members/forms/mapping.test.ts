import {
  emptyMember,
  fromMember,
  joinName,
  splitName,
  toInput,
} from "./mapping";

describe("splitting a full name", () => {
  it("takes the first word as first name, the last as last name, the rest in between", () => {
    expect(splitName("Abena Serwaa Mensah")).toEqual({
      firstName: "Abena",
      otherNames: "Serwaa",
      lastName: "Mensah",
    });
    expect(splitName("  Kwame   Asante ")).toEqual({
      firstName: "Kwame",
      otherNames: "",
      lastName: "Asante",
    });
  });

  it("leaves the last name empty until there is a second word, so the form can ask for it", () => {
    expect(splitName("Abena")).toEqual({
      firstName: "Abena",
      otherNames: "",
      lastName: "",
    });
    expect(splitName("")).toEqual({
      firstName: "",
      otherNames: "",
      lastName: "",
    });
  });
});

describe("joining a name", () => {
  it("puts the parts back together, skipping empty ones", () => {
    expect(joinName("Abena", "Serwaa", "Mensah")).toBe("Abena Serwaa Mensah");
    expect(joinName("Abena", "", "")).toBe("Abena");
    expect(joinName("", "", "")).toBe("");
  });
});

describe("member mapping", () => {
  it("round-trips a member through the form", () => {
    const member = {
      id: "m1",
      name: "Kwame Asante",
      firstName: "Kwame",
      lastName: "Asante",
      phoneNumber: "024 412 7788",
      dateOfBirth: "1988-02-14T00:00:00",
      gender: "Male" as const,
      residentialAddress: "14 Palm St",
      city: "Accra",
      hometown: "Kumasi",
      region: "GreaterAccra",
      maritalStatus: "Married" as const,
      nextOfKin: "Efua",
      emergencyContactName: "Efua",
      emergencyContactPhoneNumber: "055 301 4408",
      status: "Active" as const,
      createdAt: "2026-01-01T00:00:00Z",
    };
    const values = fromMember(member);
    expect(values.dateOfBirth).toBe("1988-02-14");
    expect(values.emailAddress).toBe("");
    expect(toInput(values)).toMatchObject({
      firstName: "Kwame",
      emailAddress: undefined,
    });
    expect(Object.keys(emptyMember)).toEqual(Object.keys(values));
  });
});
