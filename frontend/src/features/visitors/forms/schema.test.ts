import { emptyVisitor, fromVisitor, toInput, visitorSchema } from "./schema";
import type { Visitor } from "../api/visitors";

const parse = (change: Partial<typeof emptyVisitor>) =>
  visitorSchema.safeParse({
    ...emptyVisitor,
    firstName: "Esi",
    lastName: "Mensah",
    gender: "Female",
    ...change,
  });

describe("visitor form", () => {
  it("needs a name and a gender", () => {
    expect(parse({}).success).toBe(true);
    expect(parse({ gender: undefined }).success).toBe(false);
    expect(visitorSchema.safeParse(emptyVisitor).success).toBe(false);
  });

  it("checks the optional fields only when they are filled in", () => {
    expect(parse({ phoneNumber: "12" }).success).toBe(false);
    expect(parse({ phoneNumber: "055 212 3344" }).success).toBe(true);
    expect(parse({ emailAddress: "esi@" }).success).toBe(false);
    expect(parse({ notes: "x".repeat(2001) }).success).toBe(false);
  });

  it("leaves empty optional fields out of what is sent", () => {
    expect(
      toInput({
        ...emptyVisitor,
        firstName: "Esi",
        lastName: "Mensah",
        gender: "Female",
        notes: "  ",
      }),
    ).toEqual({ firstName: "Esi", lastName: "Mensah", gender: "Female" });
  });

  it("fills the edit form from a visitor, with empty text for what is missing", () => {
    const v: Visitor = {
      id: "v1",
      name: "Esi Mensah",
      firstName: "Esi",
      lastName: "Mensah",
      gender: "Female",
      createdAt: "2026-09-20T09:00:00Z",
      visitCount: 3,
    };
    expect(fromVisitor(v)).toEqual({
      firstName: "Esi",
      lastName: "Mensah",
      phoneNumber: "",
      emailAddress: "",
      gender: "Female",
      notes: "",
    });
  });
});
