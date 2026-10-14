import { emptyVisitor, fromVisitor, toInput, visitorSchema } from "./schema";
import type { Visitor } from "./api";

const parse = (change: Partial<typeof emptyVisitor>) =>
  visitorSchema.safeParse({
    ...emptyVisitor,
    firstName: "Esi",
    lastName: "Mensah",
    ...change,
  });

describe("visitor form", () => {
  it("needs only a name", () => {
    expect(parse({}).success).toBe(true);
    expect(visitorSchema.safeParse(emptyVisitor).success).toBe(false);
  });

  it("checks the optional fields only when they are filled in", () => {
    expect(parse({ phoneNumber: "12" }).success).toBe(false);
    expect(parse({ phoneNumber: "055 212 3344" }).success).toBe(true);
    expect(parse({ emailAddress: "esi@" }).success).toBe(false);
    expect(parse({ dateOfBirth: "2999-01-01" }).success).toBe(false);
    expect(parse({ dateOfBirth: "1996-08-22" }).success).toBe(true);
    expect(parse({ notes: "x".repeat(2001) }).success).toBe(false);
  });

  it("leaves empty optional fields out of what is sent", () => {
    expect(
      toInput({
        ...emptyVisitor,
        firstName: "Esi",
        lastName: "Mensah",
        notes: "  ",
      }),
    ).toEqual({ firstName: "Esi", lastName: "Mensah" });
  });

  it("fills the edit form from a visitor, with empty text for what is missing", () => {
    const v: Visitor = {
      id: "v1",
      name: "Esi Mensah",
      firstName: "Esi",
      lastName: "Mensah",
      dateOfBirth: "1996-08-22T00:00:00",
      createdAt: "2026-09-20T09:00:00Z",
      visitCount: 3,
    };
    expect(fromVisitor(v)).toEqual({
      firstName: "Esi",
      lastName: "Mensah",
      phoneNumber: "",
      emailAddress: "",
      dateOfBirth: "1996-08-22",
      notes: "",
    });
  });
});
