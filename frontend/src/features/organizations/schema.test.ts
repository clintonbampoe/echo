import {
  emptyOrganization,
  fromOrganization,
  organizationSchema,
  toInput,
} from "./schema";
import type { Organization } from "./api";

const parse = (change: Partial<typeof emptyOrganization>) =>
  organizationSchema.safeParse({
    ...emptyOrganization,
    name: "Church Choir",
    ...change,
  });

describe("organization form", () => {
  it("needs only a name", () => {
    expect(parse({}).success).toBe(true);
    expect(organizationSchema.safeParse(emptyOrganization).success).toBe(false);
    expect(parse({ name: "   " }).success).toBe(false);
  });

  it("keeps the name and description within what the server allows", () => {
    expect(parse({ name: "x".repeat(101) }).success).toBe(false);
    expect(parse({ description: "x".repeat(2001) }).success).toBe(false);
    expect(parse({ description: "x".repeat(2000) }).success).toBe(true);
  });

  it("leaves an empty description out of what is sent", () => {
    expect(toInput({ name: " Church Choir ", description: "  " })).toEqual({
      name: "Church Choir",
    });
  });

  it("fills the edit form from an organization, with empty text for what is missing", () => {
    const o: Organization = {
      id: "o1",
      name: "Church Choir",
      createdAt: "2016-02-03T09:00:00Z",
      memberCount: 61,
    };
    expect(fromOrganization(o)).toEqual({
      name: "Church Choir",
      description: "",
    });
  });
});
