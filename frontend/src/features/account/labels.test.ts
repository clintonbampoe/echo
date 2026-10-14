import { firstName, roleLabel } from "./labels";

describe("labels", () => {
  it("names the roles in plain words", () => {
    expect(roleLabel("Admin")).toBe("Administrator");
    expect(roleLabel("Clerk")).toBe("Clerk");
  });

  it("takes the first word of a name", () => {
    expect(firstName("Ama Mensah")).toBe("Ama");
    expect(firstName("  Kofi  ")).toBe("Kofi");
    expect(firstName("")).toBe("");
  });
});
