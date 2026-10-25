import { toMemberValues } from "./convert";
import { aVisitor } from "../testing/fixtures";

const visitor = aVisitor({
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  createdAt: "2026-09-07T10:00:00Z",
  visitCount: 2,
});

describe("toMemberValues", () => {
  it("carries over what the visitor record knows", () => {
    const v = toMemberValues({
      ...visitor,
      phoneNumber: "024 555 0101",
      emailAddress: "esi@example.com",
      dateOfBirth: "1995-05-02T00:00:00Z",
    });
    expect(v).toMatchObject({
      firstName: "Esi",
      lastName: "Mensah",
      phoneNumber: "024 555 0101",
      emailAddress: "esi@example.com",
      dateOfBirth: "1995-05-02",
    });
  });

  it("leaves the rest blank or on the usual choice", () => {
    expect(toMemberValues(visitor)).toMatchObject({
      phoneNumber: "",
      emailAddress: "",
      dateOfBirth: "",
      city: "",
      status: "Active",
      gender: "Male",
    });
  });
});
